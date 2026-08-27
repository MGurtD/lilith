using System.Linq.Expressions;
using Application.Contracts;
using Domain.Entities;

namespace Application.Tests.TestSupport;

/// <summary>
/// Repository that separates AddWithoutSave/AddRangeWithoutSave into a pending
/// list flushed only when <see cref="Commit"/> is called.  Used by tests that
/// assert the two-phase save behaviour in BrandingService.
///
/// Additional feature: <see cref="ThrowOnFindAsync"/> makes FindAsync throw to
/// simulate a post-commit read failure (UploadLogo_preserves_committed_branding).
/// </summary>
public sealed class StagingRepository<TEntity> : IRepository<TEntity, Guid>
    where TEntity : Entity
{
    public List<TEntity> Store { get; } = [];
    private readonly List<TEntity> _pending = [];

    /// <summary>When true, <see cref="FindAsync"/> throws to simulate a database read failure.</summary>
    public bool ThrowOnFindAsync { get; set; }

    public Task<TEntity?> Get(Guid id) =>
        Task.FromResult(Store.FirstOrDefault(entity => entity.Id == id));

    public Task<IEnumerable<TEntity>> GetAll() =>
        Task.FromResult<IEnumerable<TEntity>>(Store);

    public Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        if (ThrowOnFindAsync)
            throw new InvalidOperationException("response read failed");
        return Task.FromResult(Store.AsQueryable().Where(predicate).ToList());
    }

    public Task<List<TEntity>> FindAsyncWithQueryParams(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc)
    {
        var query = Store.AsQueryable().Where(predicate);
        return Task.FromResult((includeFunc?.Invoke(query) ?? query).ToList());
    }

    public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate) =>
        Store.AsQueryable().Where(predicate);

    public Task<bool> Exists(Guid id) =>
        Task.FromResult(Store.Any(entity => entity.Id == id));

    public Task Add(TEntity entity)
    {
        Store.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddWithoutSave(TEntity entity)
    {
        _pending.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddRange(IEnumerable<TEntity> entities)
    {
        Store.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task AddRangeWithoutSave(IEnumerable<TEntity> entities)
    {
        _pending.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task Update(TEntity entity) => Task.CompletedTask;
    public bool UpdateWithoutSave(TEntity entity) => true;

    public Task Remove(TEntity entity)
    {
        Store.Remove(entity);
        _pending.Remove(entity);
        return Task.CompletedTask;
    }

    public Task RemoveRange(IEnumerable<TEntity> entities)
    {
        foreach (var e in entities.ToList())
        {
            Store.Remove(e);
            _pending.Remove(e);
        }
        return Task.CompletedTask;
    }

    public Task SaveChanges() => Task.CompletedTask;

    /// <summary>Flushes the pending list into the main Store (mirrors EF SaveChanges).</summary>
    public void Commit()
    {
        Store.AddRange(_pending);
        _pending.Clear();
    }
}
