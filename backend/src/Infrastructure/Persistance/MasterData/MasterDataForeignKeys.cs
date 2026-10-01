using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.Persistance.MasterData;

/// <summary>
/// Makes the database the last line of defence for master data: every foreign key that
/// would delete in cascade from master data to anything other than one of its owned parts
/// becomes <see cref="DeleteBehavior.Restrict"/>. A filtered owned part, such as empty
/// stock, is restricted too, because a foreign key cannot tell the parts that go with their
/// owner from those that keep it in use; the repository deletes it explicitly.
/// <see cref="MasterDataDeleteGuard"/> refuses first and names the documents; this only
/// stops deletes that bypass it.
/// </summary>
internal static class MasterDataForeignKeys
{
    public static void Restrict(IMutableModel model)
    {
        var ownedParts = MasterDataCatalog.OwnedParts
            .Where(p => p.Filter is null)
            .Select(p => (p.Owner, p.Part))
            .ToHashSet();

        var cascades = model.GetEntityTypes()
            .SelectMany(t => t.GetDeclaredForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade
                && typeof(IMasterData).IsAssignableFrom(fk.PrincipalEntityType.ClrType)
                && !ownedParts.Contains((fk.PrincipalEntityType.ClrType, fk.DeclaringEntityType.ClrType)))
            .ToList();

        foreach (var foreignKey in cascades)
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
