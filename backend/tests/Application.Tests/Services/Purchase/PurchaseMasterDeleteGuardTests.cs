using Application.Contracts;
using Application.Contracts.Services.Geolocalization;
using Application.Services.Purchase;
using Application.Tests.TestSupport;
using Domain.Entities.Purchase;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Purchase;

/// <summary>
/// Expense types, supplier types and suppliers in use must not be deleted (#159):
/// expenses, suppliers, purchase orders and invoices cascade from them, and
/// receipts have no foreign key and would be orphaned.
/// </summary>
public class PurchaseMasterDeleteGuardTests
{
    private static readonly Dictionary<string, string> LocalizationKeys = new()
    {
        ["ExpenseTypeInUse"] = "El tipus de despesa {0} està en ús",
        ["SupplierTypeInUse"] = "El tipus de proveïdor {0} està en ús",
        ["SupplierInUse"] = "El proveïdor {0} està en ús",
        ["EntityNotFound"] = "L'entitat amb ID {0} no existeix",
    };

    private static KeyedLocalizationService Localization() => new(LocalizationKeys);

    [Fact]
    public async Task RemoveExpenseType_refuses_a_type_with_expenses()
    {
        var type = new ExpenseType { Name = "Lloguer" };
        var (sut, repository) = BuildExpenseTypeSut(type, inUse: true);

        var response = await sut.RemoveExpenseType(type.Id);

        Assert.False(response.Result);
        Assert.Equal("El tipus de despesa Lloguer està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<ExpenseType>());
    }

    [Fact]
    public async Task RemoveExpenseType_deletes_an_unused_type()
    {
        var type = new ExpenseType { Name = "Lloguer" };
        var (sut, repository) = BuildExpenseTypeSut(type, inUse: false);

        var response = await sut.RemoveExpenseType(type.Id);

        Assert.True(response.Result);
        await repository.Received(1).Remove(type);
    }

    [Fact]
    public async Task RemoveSupplierType_refuses_a_type_with_suppliers()
    {
        var type = new SupplierType { Name = "Transport" };
        var (sut, repository) = BuildSupplierTypeSut(type, inUse: true);

        var response = await sut.RemoveSupplierType(type.Id);

        Assert.False(response.Result);
        Assert.Equal("El tipus de proveïdor Transport està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<SupplierType>());
    }

    [Fact]
    public async Task RemoveSupplierType_deletes_an_unused_type()
    {
        var type = new SupplierType { Name = "Transport" };
        var (sut, repository) = BuildSupplierTypeSut(type, inUse: false);

        var response = await sut.RemoveSupplierType(type.Id);

        Assert.True(response.Result);
        await repository.Received(1).Remove(type);
    }

    [Fact]
    public async Task RemoveSupplier_refuses_a_supplier_in_use()
    {
        var supplier = new Supplier { ComercialName = "Acers Vallès" };
        var (sut, repository) = BuildSupplierSut(supplier, inUse: true);

        var response = await sut.RemoveSupplier(supplier.Id);

        Assert.False(response.Result);
        Assert.Equal("El proveïdor Acers Vallès està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<Supplier>());
    }

    [Fact]
    public async Task RemoveSupplier_deletes_an_unused_supplier()
    {
        var supplier = new Supplier { ComercialName = "Acers Vallès" };
        var (sut, repository) = BuildSupplierSut(supplier, inUse: false);

        var response = await sut.RemoveSupplier(supplier.Id);

        Assert.True(response.Result);
        await repository.Received(1).Remove(supplier);
    }

    private static (ExpenseTypeService, IExpenseTypeRepository) BuildExpenseTypeSut(ExpenseType type, bool inUse)
    {
        var repository = Substitute.For<IExpenseTypeRepository>().Serving(type);
        repository.IsInUse(type.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.ExpenseTypes.Returns(repository);
        return (new ExpenseTypeService(unitOfWork, Localization()), repository);
    }

    private static (SupplierTypeService, ISupplierTypeRepository) BuildSupplierTypeSut(SupplierType type, bool inUse)
    {
        var repository = Substitute.For<ISupplierTypeRepository>().Serving(type);
        repository.IsInUse(type.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.SupplierTypes.Returns(repository);
        return (new SupplierTypeService(unitOfWork, Localization()), repository);
    }

    private static (SupplierService, ISupplierRepository) BuildSupplierSut(Supplier supplier, bool inUse)
    {
        var repository = Substitute.For<ISupplierRepository>().Serving(supplier);
        repository.IsInUse(supplier.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Suppliers.Returns(repository);
        var sut = new SupplierService(
            unitOfWork,
            Localization(),
            Substitute.For<IGeolocalizationService>(),
            NullLogger<SupplierService>.Instance);
        return (sut, repository);
    }
}
