using System.Linq.Expressions;
using System.Reflection;
using Application.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;

namespace Infrastructure.Persistance.MasterData;

/// <summary>
/// Refuses to physically delete master data that is still in use. Who refers to master
/// data is derived from the EF model, plus the declarations in <see cref="MasterDataCatalog"/>.
/// A record is in use when anything outside the set being deleted (the record and its owned
/// parts, recursively) still refers to that set. Foreign keys that set the reference to
/// null on delete never keep a record in use.
/// </summary>
internal static class MasterDataDeleteGuard
{
    private static readonly Dictionary<Type, string> DocumentKindKeys = MasterDataCatalog.DocumentKinds
        .GroupBy(k => k.Type)
        .ToDictionary(g => g.Key, g => g.First().Key);

    private static readonly List<string> DocumentKindOrder = MasterDataCatalog.DocumentKinds
        .Select(k => k.Key)
        .Distinct()
        .ToList();

    /// <summary>
    /// Throws <see cref="EntityInUseException"/> when the master data in
    /// <paramref name="entities"/> is in use, without changing the context. Otherwise returns
    /// the ids of the owned parts that go with it, for <see cref="RemoveOwnedParts"/>.
    /// </summary>
    public static async Task<IReadOnlyDictionary<Type, List<Guid>>> EnsureNotInUse(
        ApplicationDbContext context, IEnumerable<Entity> entities)
    {
        var roots = entities.Where(e => e is IMasterData).ToList();
        if (roots.Count == 0)
            return new Dictionary<Type, List<Guid>>();

        var deleted = await CollectDeletedSet(context, roots);
        var kinds = await FindDocumentKindsInUse(context, deleted);
        if (kinds.Count > 0)
            throw new EntityInUseException(roots.Count == 1 ? NameOf(roots[0]) : null, kinds, CanBeDisabled(roots));

        var rootIds = roots.Select(r => r.Id).ToHashSet();
        return deleted
            .Select(d => (Type: d.Key, Ids: d.Value.Where(id => !rootIds.Contains(id)).ToList()))
            .Where(d => d.Ids.Count > 0)
            .ToDictionary(d => d.Type, d => d.Ids);
    }

    /// <summary>
    /// Marks the owned parts as deleted, so EF deletes them in order with their owner. A
    /// filtered part, such as empty stock, is not deleted in cascade by the database (see
    /// <see cref="MasterDataForeignKeys"/>), so only this explicit delete removes it. Call it
    /// after removing the owner, so parts the owner brought into the context are reused.
    /// </summary>
    public static async Task RemoveOwnedParts(ApplicationDbContext context, IReadOnlyDictionary<Type, List<Guid>> parts)
    {
        foreach (var (type, ids) in parts)
            context.RemoveRange(await Invoke<Task<List<Entity>>>(nameof(LoadParts), type, context, ids));
    }

    /// <summary>A reference to one entity type: the referring type and its property.</summary>
    internal sealed record IncomingReference(Type Dependent, string Property, Type PropertyType);

    /// <summary>Every reference that keeps a record of <paramref name="principal"/> in use.</summary>
    internal static IEnumerable<IncomingReference> IncomingReferences(IModel model, Type principal)
    {
        var entityType = model.FindEntityType(principal);
        if (entityType is not null)
        {
            foreach (var foreignKey in entityType.GetReferencingForeignKeys())
            {
                if (foreignKey.DeleteBehavior == DeleteBehavior.SetNull || foreignKey.Properties.Count != 1)
                    continue;
                var property = foreignKey.Properties[0];
                yield return new IncomingReference(foreignKey.DeclaringEntityType.ClrType, property.Name, property.ClrType);
            }
        }

        foreach (var extra in MasterDataCatalog.ExtraReferences.Where(r => r.Principal == principal))
        {
            var property = extra.Dependent.GetProperty(extra.Property)
                ?? throw new InvalidOperationException($"{extra.Dependent.Name} has no property {extra.Property}.");
            yield return new IncomingReference(extra.Dependent, extra.Property, property.PropertyType);
        }
    }

    /// <summary>The foreign key property through which an owned part refers to its owner.</summary>
    internal static IncomingReference OwnerReference(IModel model, OwnedPart part)
    {
        var references = IncomingReferences(model, part.Owner).Where(r => r.Dependent == part.Part).ToList();
        return references.Count == 1
            ? references[0]
            : throw new InvalidOperationException(
                $"{part.Part.Name} must refer to {part.Owner.Name} through exactly one foreign key, found {references.Count}.");
    }

    /// <summary>
    /// The same refusal as <see cref="EnsureNotInUse"/> when a delete bypassed the guard and
    /// PostgreSQL refused it through a foreign key from master data
    /// (see <see cref="MasterDataForeignKeys"/>); null for any other failure.
    /// </summary>
    internal static EntityInUseException? DatabaseRefusal(IModel model, DbUpdateException exception)
    {
        if (exception.InnerException is not PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } refusal)
            return null;

        var foreignKey = model.GetEntityTypes()
            .SelectMany(t => t.GetDeclaredForeignKeys())
            .FirstOrDefault(fk => fk.GetConstraintName() == refusal.ConstraintName);
        if (foreignKey is null || !typeof(IMasterData).IsAssignableFrom(foreignKey.PrincipalEntityType.ClrType))
            return null;

        // Without a deleted master, an insert or update referred to a record that does not exist.
        var deleted = exception.Entries
            .Where(e => e.State == EntityState.Deleted && e.Metadata.ClrType == foreignKey.PrincipalEntityType.ClrType)
            .Select(e => (Entity)e.Entity)
            .ToList();
        if (deleted.Count == 0)
            return null;

        var kind = DocumentKindKey(foreignKey.DeclaringEntityType.ClrType) ?? "DocumentKind.Other";
        return new EntityInUseException(deleted.Count == 1 ? NameOf(deleted[0]) : null, [kind], CanBeDisabled(deleted), exception);
    }

    internal static string? DocumentKindKey(Type type) => DocumentKindKeys.GetValueOrDefault(type);

    private static bool CanBeDisabled(IEnumerable<Entity> entities) =>
        entities.All(e => MasterDataCatalog.CanBeDisabled.Contains(e.GetType()));

    private static string? NameOf(Entity entity) =>
        MasterDataCatalog.Names.TryGetValue(entity.GetType(), out var name) ? name(entity) : null;

    private static async Task<Dictionary<Type, HashSet<Guid>>> CollectDeletedSet(ApplicationDbContext context, List<Entity> roots)
    {
        var deleted = new Dictionary<Type, HashSet<Guid>>();
        var pending = new Queue<(Type Type, List<Guid> Ids)>();

        foreach (var group in roots.GroupBy(e => e.GetType()))
        {
            var ids = group.Select(e => e.Id).ToList();
            Add(deleted, group.Key, ids);
            pending.Enqueue((group.Key, ids));
        }

        while (pending.Count > 0)
        {
            var (owner, ownerIds) = pending.Dequeue();
            foreach (var part in MasterDataCatalog.OwnedParts.Where(p => p.Owner == owner))
            {
                var reference = OwnerReference(context.Model, part);
                var partIds = await Invoke<Task<List<Guid>>>(nameof(IdsReferring), part.Part,
                    context, reference, ownerIds, part.Filter);
                var added = Add(deleted, part.Part, partIds);
                if (added.Count > 0)
                    pending.Enqueue((part.Part, added));
            }
        }

        return deleted;
    }

    private static async Task<List<string>> FindDocumentKindsInUse(ApplicationDbContext context, Dictionary<Type, HashSet<Guid>> deleted)
    {
        var kinds = new HashSet<string>();
        foreach (var (principal, ids) in deleted)
        {
            var idList = ids.ToList();
            foreach (var reference in IncomingReferences(context.Model, principal))
            {
                var kind = DocumentKindKey(reference.Dependent) ?? "DocumentKind.Other";
                if (kinds.Contains(kind))
                    continue;

                var excluded = deleted.TryGetValue(reference.Dependent, out var set) ? set.ToList() : [];
                if (await Invoke<Task<bool>>(nameof(AnyReferring), reference.Dependent, context, reference, idList, excluded))
                    kinds.Add(kind);
            }
        }

        return kinds.OrderBy(k => DocumentKindOrder.IndexOf(k) is var i and >= 0 ? i : int.MaxValue).ToList();
    }

    private static List<Guid> Add(Dictionary<Type, HashSet<Guid>> deleted, Type type, IEnumerable<Guid> ids)
    {
        if (!deleted.TryGetValue(type, out var set))
            deleted[type] = set = [];
        return ids.Where(set.Add).ToList();
    }

    private static T Invoke<T>(string method, Type entityType, params object?[] arguments) =>
        (T)typeof(MasterDataDeleteGuard)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(entityType)
            .Invoke(null, arguments)!;

    private static Task<List<Guid>> IdsReferring<TDependent>(
        ApplicationDbContext context, IncomingReference reference, List<Guid> ids, LambdaExpression? filter)
        where TDependent : Entity
    {
        var query = Referring<TDependent>(context, reference, ids);
        if (filter is not null)
            query = query.Where((Expression<Func<TDependent, bool>>)filter);
        return query.Select(e => e.Id).ToListAsync();
    }

    private static Task<bool> AnyReferring<TDependent>(
        ApplicationDbContext context, IncomingReference reference, List<Guid> ids, List<Guid> excluded)
        where TDependent : Entity
    {
        var query = Referring<TDependent>(context, reference, ids);
        if (excluded.Count > 0)
            query = query.Where(e => !excluded.Contains(e.Id));
        return query.AnyAsync();
    }

    // Disabled records still hold their foreign keys, so query filters are ignored.
    private static IQueryable<TDependent> Referring<TDependent>(
        ApplicationDbContext context, IncomingReference reference, List<Guid> ids)
        where TDependent : Entity
    {
        var entity = Expression.Parameter(typeof(TDependent), "e");
        var value = Expression.Call(
            typeof(EF), nameof(EF.Property), [reference.PropertyType],
            entity, Expression.Constant(reference.Property));
        var nullable = reference.PropertyType == typeof(Guid?);
        var holder = nullable
            ? (object)new IdList<Guid?>(ids.Select(id => (Guid?)id).ToList())
            : new IdList<Guid>(ids);
        var list = Expression.Property(Expression.Constant(holder), "Ids");
        var contains = Expression.Call(
            typeof(Enumerable), nameof(Enumerable.Contains), [reference.PropertyType], list, value);

        return context.Set<TDependent>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(Expression.Lambda<Func<TDependent, bool>>(contains, entity));
    }

    // Tracked, so records already in the context are reused instead of attached twice.
    private static async Task<List<Entity>> LoadParts<TPart>(ApplicationDbContext context, List<Guid> ids)
        where TPart : Entity
    {
        var parts = await context.Set<TPart>().IgnoreQueryFilters().Where(e => ids.Contains(e.Id)).ToListAsync();
        return parts.Cast<Entity>().ToList();
    }

    // Reading the ids through a property lets EF send them as a query parameter.
    private sealed record IdList<T>(List<T> Ids);
}
