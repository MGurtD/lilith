using Application.Contracts;
using Application.Contracts.Services.Geolocalization;
using Application.Services.Sales;
using Application.Tests.TestSupport;
using Domain.Entities.Sales;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.Sales;

/// <summary>
/// Customer types and customers in use must not be deleted (#159): customers and
/// their delivery notes cascade from them.
/// </summary>
public class SalesMasterDeleteGuardTests
{
    private static readonly Dictionary<string, string> LocalizationKeys = new()
    {
        ["CustomerTypeInUse"] = "El tipus de client {0} està en ús",
        ["CustomerInUse"] = "El client {0} està en ús",
        ["EntityNotFound"] = "L'entitat amb ID {0} no existeix",
    };

    private static KeyedLocalizationService Localization() => new(LocalizationKeys);

    [Fact]
    public async Task RemoveCustomerType_refuses_a_type_with_customers()
    {
        var type = new CustomerType { Name = "Distribuïdor" };
        var (sut, repository) = BuildCustomerTypeSut(type, inUse: true);

        var response = await sut.RemoveCustomerType(type.Id);

        Assert.False(response.Result);
        Assert.Equal("El tipus de client Distribuïdor està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<CustomerType>());
    }

    [Fact]
    public async Task RemoveCustomerType_deletes_an_unused_type()
    {
        var type = new CustomerType { Name = "Distribuïdor" };
        var (sut, repository) = BuildCustomerTypeSut(type, inUse: false);

        var response = await sut.RemoveCustomerType(type.Id);

        Assert.True(response.Result);
        await repository.Received(1).Remove(type);
    }

    [Fact]
    public async Task RemoveCustomer_refuses_a_customer_in_use()
    {
        var customer = new Customer { ComercialName = "Mobles Pla" };
        var (sut, repository) = BuildCustomerSut(customer, inUse: true);

        var response = await sut.RemoveCustomer(customer.Id);

        Assert.False(response.Result);
        Assert.Equal("El client Mobles Pla està en ús", Assert.Single(response.Errors));
        await repository.DidNotReceive().Remove(Arg.Any<Customer>());
    }

    [Fact]
    public async Task RemoveCustomer_deletes_an_unused_customer()
    {
        var customer = new Customer { ComercialName = "Mobles Pla" };
        var (sut, repository) = BuildCustomerSut(customer, inUse: false);

        var response = await sut.RemoveCustomer(customer.Id);

        Assert.True(response.Result);
        await repository.Received(1).Remove(customer);
    }

    private static (CustomerTypeService, ICustomerTypeRepository) BuildCustomerTypeSut(CustomerType type, bool inUse)
    {
        var repository = Substitute.For<ICustomerTypeRepository>().Serving(type);
        repository.IsInUse(type.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.CustomerTypes.Returns(repository);
        return (new CustomerTypeService(unitOfWork, Localization()), repository);
    }

    private static (CustomerService, ICustomerRepository) BuildCustomerSut(Customer customer, bool inUse)
    {
        var repository = Substitute.For<ICustomerRepository>().Serving(customer);
        repository.IsInUse(customer.Id).Returns(inUse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Customers.Returns(repository);
        var sut = new CustomerService(
            unitOfWork,
            Localization(),
            Substitute.For<IGeolocalizationService>(),
            NullLogger<CustomerService>.Instance);
        return (sut, repository);
    }
}
