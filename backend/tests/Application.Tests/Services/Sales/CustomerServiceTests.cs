using Application.Contracts;
using Application.Contracts.Services.Geolocalization;
using Application.Services.Sales;
using Application.Tests.TestData;
using Application.Tests.TestSupport;
using Domain.Entities.Sales;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services.Sales;

/// <summary>
/// Unit tests for <see cref="CustomerService"/> — issue #69 follow-up.
/// Validates that Create/Update block on invalid fiscal data (CIF/NIF or
/// incomplete fiscal address) and that Update returns EntityNotFound for
/// missing customers.
/// </summary>
public class CustomerServiceTests
{
    private const string InvalidVat = "ABC123";

    // Localization keys used by CustomerService
    private static readonly Dictionary<string, string> LocalizationKeys = new()
    {
        ["CustomerCifInvalid"]            = "CIF/NIF invàlid",
        ["CustomerFiscalAddressInvalid"]  = "La direcció fiscal principal del client és incompleta. Cal omplir país, codi postal, ciutat i adreça",
        ["CustomerInvalid"]               = "El client no és vàlid per a crear una factura. Revisa el nom fiscal, el número de compte i el NIF",
        ["CustomerNoAddresses"]           = "El client no té direccions donades d'alta. Si us plau, crei una direcció.",
        ["CustomerAlreadyExists"]         = "Client {0} existent",
        ["EntityNotFound"]                = "L'entitat amb ID {0} no existeix",
    };

    // -------- Create --------

    [Fact]
    public async Task CreateCustomer_blocks_when_vatNumber_is_invalid()
    {
        var context = BuildSut();
        var customer = CustomerBuilder.Valid(InvalidVat);

        var response = await context.Sut.CreateCustomer(customer);

        Assert.False(response.Result);
        Assert.Equal("CIF/NIF invàlid", Assert.Single(response.Errors));
        Assert.Equal(1, context.Localization.LookupCount);
    }

    [Fact]
    public async Task CreateCustomer_blocks_when_fiscal_address_is_incomplete()
    {
        var context = BuildSut();
        var customer = CustomerBuilder.Valid();
        customer.Address.Clear();
        customer.Address.Add(CustomerBuilder.IncompleteAddress(customer.Id, "Country"));

        var response = await context.Sut.CreateCustomer(customer);

        Assert.False(response.Result);
        Assert.Equal(
            "La direcció fiscal principal del client és incompleta. Cal omplir país, codi postal, ciutat i adreça",
            Assert.Single(response.Errors));
        Assert.Equal(1, context.Localization.LookupCount);
    }

    [Fact]
    public async Task CreateCustomer_succeeds_when_vat_and_address_valid()
    {
        var context = BuildSut();
        var customer = CustomerBuilder.Valid();

        var response = await context.Sut.CreateCustomer(customer);

        Assert.True(response.Result);
        Assert.Equal(customer, response.Content);
        Assert.Contains(customer, context.AddedCustomers);
    }

    // -------- Update --------

    [Fact]
    public async Task UpdateCustomer_blocks_when_vatNumber_is_invalid()
    {
        var existing = CustomerBuilder.Valid();
        var context = BuildSut(existing);

        var modified = CustomerBuilder.Valid(InvalidVat);
        modified.Id = existing.Id;

        var response = await context.Sut.UpdateCustomer(modified);

        Assert.False(response.Result);
        Assert.Equal("CIF/NIF invàlid", Assert.Single(response.Errors));
        Assert.Empty(context.UpdatedCustomers);
    }

    [Fact]
    public async Task UpdateCustomer_blocks_when_fiscal_address_is_incomplete()
    {
        var existing = CustomerBuilder.Valid();
        var context = BuildSut(existing);

        var modified = CustomerBuilder.Valid();
        modified.Id = existing.Id;
        modified.Address.Clear();
        modified.Address.Add(CustomerBuilder.IncompleteAddress(modified.Id, "PostalCode"));

        var response = await context.Sut.UpdateCustomer(modified);

        Assert.False(response.Result);
        Assert.Equal(
            "La direcció fiscal principal del client és incompleta. Cal omplir país, codi postal, ciutat i adreça",
            Assert.Single(response.Errors));
        Assert.Empty(context.UpdatedCustomers);
    }

    [Fact]
    public async Task UpdateCustomer_returns_EntityNotFound_when_customer_does_not_exist()
    {
        var context = BuildSut();
        var modified = CustomerBuilder.Valid();
        modified.Id = Guid.NewGuid();

        var response = await context.Sut.UpdateCustomer(modified);

        Assert.False(response.Result);
        Assert.Contains("L'entitat amb ID", Assert.Single(response.Errors));
        Assert.Empty(context.UpdatedCustomers);
    }

    [Fact]
    public async Task UpdateCustomer_succeeds_when_vat_and_address_valid()
    {
        var existing = CustomerBuilder.Valid();
        var context = BuildSut(existing);

        var modified = CustomerBuilder.Valid();
        modified.Id = existing.Id;
        modified.ComercialName = "Updated Name";

        var response = await context.Sut.UpdateCustomer(modified);

        Assert.True(response.Result);
        Assert.Contains(modified, context.UpdatedCustomers);
    }

    // -------- helpers --------

    private static TestContext BuildSut(params Customer[] seed)
    {
        var store = seed.ToList();
        var added = new List<Customer>();
        var updated = new List<Customer>();
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository
            .Find(Arg.Any<Expression<Func<Customer, bool>>>())
            .Returns(call => store.AsQueryable().Where(call.Arg<Expression<Func<Customer, bool>>>()).ToList());
        customerRepository
            .Get(Arg.Any<Guid>())
            .Returns(call => store.FirstOrDefault(customer => customer.Id == call.Arg<Guid>()));
        customerRepository
            .Add(Arg.Any<Customer>())
            .Returns(call =>
            {
                var customer = call.Arg<Customer>();
                store.Add(customer);
                added.Add(customer);
                return Task.CompletedTask;
            });
        customerRepository
            .Update(Arg.Any<Customer>())
            .Returns(call =>
            {
                updated.Add(call.Arg<Customer>());
                return Task.CompletedTask;
            });

        var uow = Substitute.For<IUnitOfWork>();
        uow.Customers.Returns(customerRepository);

        var localization = new KeyedLocalizationService(LocalizationKeys);
        var sut = new CustomerService(
            uow,
            localization,
            Substitute.For<IGeolocalizationService>(),
            NullLogger<CustomerService>.Instance);

        return new TestContext(sut, localization, added, updated);
    }

    private sealed record TestContext(
        CustomerService Sut,
        KeyedLocalizationService Localization,
        List<Customer> AddedCustomers,
        List<Customer> UpdatedCustomers);
}
