using Application.Contracts;
using Domain.Entities.Sales;
using Infrastructure.Persistance;
using Infrastructure.Persistance.MasterData;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Application.Tests.Persistance;

/// <summary>
/// When a delete bypasses the guard, PostgreSQL refuses it through a restricted foreign key
/// and the refusal reaches the user as the same <see cref="EntityInUseException"/>. Uses the
/// PostgreSQL model, which is built without connecting, so constraint names are the real ones.
/// </summary>
public class MasterDataForeignKeysTests
{
    [Fact]
    public void A_refused_delete_of_master_data_names_the_record_and_the_document_kind()
    {
        using var context = NewContext();
        var customer = new Customer { Id = Guid.NewGuid(), ComercialName = "ACME" };
        context.Attach(customer);
        context.Remove(customer);

        var refusal = MasterDataDeleteGuard.DatabaseRefusal(context.Model,
            Failure(context, ConstraintName<Budget, Customer>(context), PostgresErrorCodes.ForeignKeyViolation));

        Assert.NotNull(refusal);
        Assert.Equal("ACME", refusal.EntityName);
        Assert.Equal(["DocumentKind.Budgets"], refusal.DocumentKindKeys);
    }

    [Fact]
    public void A_record_referring_to_missing_master_data_is_not_a_refused_delete()
    {
        using var context = NewContext();
        context.Add(new Budget { Id = Guid.NewGuid(), Number = "P-1", CustomerId = Guid.NewGuid() });

        var refusal = MasterDataDeleteGuard.DatabaseRefusal(context.Model,
            Failure(context, ConstraintName<Budget, Customer>(context), PostgresErrorCodes.ForeignKeyViolation));

        Assert.Null(refusal);
    }

    [Fact]
    public void Other_database_errors_are_not_a_refused_delete()
    {
        using var context = NewContext();
        var customer = new Customer { Id = Guid.NewGuid() };
        context.Attach(customer);
        context.Remove(customer);

        var refusal = MasterDataDeleteGuard.DatabaseRefusal(context.Model,
            Failure(context, ConstraintName<Budget, Customer>(context), PostgresErrorCodes.UniqueViolation));

        Assert.Null(refusal);
    }

    private static string ConstraintName<TDependent, TPrincipal>(DbContext context) =>
        context.Model.FindEntityType(typeof(TDependent))!.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(TPrincipal))
            .GetConstraintName()!;

    private static DbUpdateException Failure(DbContext context, string constraint, string sqlState) =>
        new("An error occurred while saving the entity changes.",
            new PostgresException("violation", "ERROR", "ERROR", sqlState, constraintName: constraint),
            context.ChangeTracker.Entries().ToList());

    private static ApplicationDbContext NewContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=model-only")
            .Options);
}
