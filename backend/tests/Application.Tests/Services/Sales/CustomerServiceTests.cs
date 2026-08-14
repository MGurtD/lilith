using Application.Services.Sales;
using Application.Contracts;
using Application.Contracts.Persistance.Repositories.Purchase;
using Application.Contracts.Services.Geolocalization;
using Domain.Entities;
using Domain.Entities.Auth;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Domain.Entities.Transport;
using Domain.Entities.Warehouse;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services.Sales;

/// <summary>
/// Unit tests for <see cref="CustomerService"/> — issue #69 follow-up.
/// Validates that Create/Update block on invalid fiscal data (CIF/NIF or
/// incomplete fiscal address) and that Update returns EntityNotFound for
/// missing customers. Hand-rolled fakes are used because the test project
/// has no Moq/NSubstitute dependency.
/// </summary>
public class CustomerServiceTests
{
    private const string ValidVat = "12345678Z";
    private const string InvalidVat = "ABC123";

    // -------- Create --------

    [Fact]
    public async Task CreateCustomer_blocks_when_vatNumber_is_invalid()
    {
        var (sut, _, localization, _) = BuildSut(seedCustomers: Array.Empty<Customer>());
        var customer = NewValidCustomer(vatNumber: InvalidVat);

        var response = await sut.CreateCustomer(customer);

        Assert.False(response.Result);
        Assert.Equal("CIF/NIF invàlid", Assert.Single(response.Errors));
        Assert.Equal(1, localization.LookupCount);
    }

    [Fact]
    public async Task CreateCustomer_blocks_when_fiscal_address_is_incomplete()
    {
        var (sut, _, localization, _) = BuildSut(seedCustomers: Array.Empty<Customer>());
        var customer = NewValidCustomer();
        customer.Address.Clear();
        customer.Address.Add(new CustomerAddress
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Main = true,
            Country = "",   // missing → fails
            PostalCode = "08001",
            City = "Barcelona",
            Address = "C/ Major 1",
        });

        var response = await sut.CreateCustomer(customer);

        Assert.False(response.Result);
        Assert.Equal(
            "La direcció fiscal principal del client és incompleta. Cal omplir país, codi postal, ciutat i adreça",
            Assert.Single(response.Errors));
        Assert.Equal(1, localization.LookupCount);
    }

    [Fact]
    public async Task CreateCustomer_succeeds_when_vat_and_address_valid()
    {
        var (sut, uow, _, _) = BuildSut(seedCustomers: Array.Empty<Customer>());
        var customer = NewValidCustomer();

        var response = await sut.CreateCustomer(customer);

        Assert.True(response.Result);
        Assert.Equal(customer, response.Content);
        Assert.Contains(customer, uow.CustomersStore.Added);
    }

    // -------- Update --------

    [Fact]
    public async Task UpdateCustomer_blocks_when_vatNumber_is_invalid()
    {
        var existing = NewValidCustomer();
        var (sut, uow, _, _) = BuildSut(seedCustomers: new[] { existing });

        var modified = NewValidCustomer(vatNumber: InvalidVat);
        modified.Id = existing.Id;

        var response = await sut.UpdateCustomer(modified);

        Assert.False(response.Result);
        Assert.Equal("CIF/NIF invàlid", Assert.Single(response.Errors));
        Assert.Empty(uow.CustomersStore.Updated);
    }

    [Fact]
    public async Task UpdateCustomer_blocks_when_fiscal_address_is_incomplete()
    {
        var existing = NewValidCustomer();
        var (sut, uow, _, _) = BuildSut(seedCustomers: new[] { existing });

        var modified = NewValidCustomer();
        modified.Id = existing.Id;
        modified.Address.Clear();
        modified.Address.Add(new CustomerAddress
        {
            Id = Guid.NewGuid(),
            CustomerId = modified.Id,
            Main = true,
            Country = "Espanya",
            PostalCode = "",  // missing → fails
            City = "Barcelona",
            Address = "C/ Major 1",
        });

        var response = await sut.UpdateCustomer(modified);

        Assert.False(response.Result);
        Assert.Equal(
            "La direcció fiscal principal del client és incompleta. Cal omplir país, codi postal, ciutat i adreça",
            Assert.Single(response.Errors));
        Assert.Empty(uow.CustomersStore.Updated);
    }

    [Fact]
    public async Task UpdateCustomer_returns_EntityNotFound_when_customer_does_not_exist()
    {
        var (sut, uow, _, _) = BuildSut(seedCustomers: Array.Empty<Customer>());
        var modified = NewValidCustomer();
        modified.Id = Guid.NewGuid();

        var response = await sut.UpdateCustomer(modified);

        Assert.False(response.Result);
        Assert.Contains("L'entitat amb ID", Assert.Single(response.Errors));
        Assert.Empty(uow.CustomersStore.Updated);
    }

    [Fact]
    public async Task UpdateCustomer_succeeds_when_vat_and_address_valid()
    {
        var existing = NewValidCustomer();
        var (sut, uow, _, _) = BuildSut(seedCustomers: new[] { existing });

        var modified = NewValidCustomer();
        modified.Id = existing.Id;
        modified.ComercialName = "Updated Name";

        var response = await sut.UpdateCustomer(modified);

        Assert.True(response.Result);
        Assert.Contains(modified, uow.CustomersStore.Updated);
    }

    // -------- helpers --------

    private static Customer NewValidCustomer(string? vatNumber = null)
    {
        var id = Guid.NewGuid();
        return new Customer
        {
            Id = id,
            Code = "C001",
            ComercialName = "Acme SA",
            TaxName = "Acme Sociedad Anónima",
            VatNumber = vatNumber ?? ValidVat,
            AccountNumber = "ES7621000000000000000000",
            Address = new List<CustomerAddress>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = id,
                    Main = true,
                    Country = "Espanya",
                    PostalCode = "08001",
                    City = "Barcelona",
                    Address = "C/ Major 1",
                },
            },
        };
    }

    private static (CustomerService, FakeUnitOfWork, FakeLocalizationService, NullLogger<CustomerService>)
        BuildSut(IEnumerable<Customer> seedCustomers)
    {
        var uow = new FakeUnitOfWork(seedCustomers);
        var localization = new FakeLocalizationService();
        var geo = new FakeGeolocalizationService();
        var logger = NullLogger<CustomerService>.Instance;
        var sut = new CustomerService(uow, localization, geo, logger);
        return (sut, uow, localization, logger);
    }

    // -------- hand-rolled fakes --------

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeCustomerRepository CustomersStore { get; }
        ICustomerRepository IUnitOfWork.Customers => CustomersStore;

        public FakeUnitOfWork(IEnumerable<Customer> seed) =>
            CustomersStore = new FakeCustomerRepository(seed);

        public Task<int> CompleteAsync() => Task.FromResult(0);
        public void Dispose() { }

        // All other repositories throw on access — they are never reached by
        // CustomerService so we surface accidental coupling loudly in tests.
        public IRepository<Role, Guid> Roles => throw new NotImplementedException();
        public IRepository<User, Guid> Users => throw new NotImplementedException();
        public IRepository<ApiKey, Guid> ApiKeys => throw new NotImplementedException();
        public IRepository<UserRefreshToken, Guid> UserRefreshTokens => throw new NotImplementedException();
        public IRepository<UserFilter, Guid> UserFilters => throw new NotImplementedException();
        public IRepository<UserTableView, Guid> UserTableViews => throw new NotImplementedException();
        public IRepository<Profile, Guid> Profiles => throw new NotImplementedException();
        public IRepository<MenuItem, Guid> MenuItems => throw new NotImplementedException();
        public IRepository<MenuItemTranslation, Guid> MenuItemTranslations => throw new NotImplementedException();
        public IRepository<ProfileMenuItem, Guid> ProfileMenuItems => throw new NotImplementedException();
        public IRepository<Domain.Entities.File, Guid> Files => throw new NotImplementedException();
        public IRepository<Parameter, Guid> Parameters => throw new NotImplementedException();
        public IRepository<Exercise, Guid> Exercices => throw new NotImplementedException();
        public IRepository<Tax, Guid> Taxes => throw new NotImplementedException();
        public IRepository<PaymentMethod, Guid> PaymentMethods => throw new NotImplementedException();
        public ILifecycleRepository Lifecycles => throw new NotImplementedException();
        public ILifecycleTagRepository LifecycleTags => throw new NotImplementedException();
        public IRepository<StatusLifecycleTag, Guid> StatusLifecycleTags => throw new NotImplementedException();
        public IRepository<SupplierType, Guid> SupplierTypes => throw new NotImplementedException();
        public ISupplierRepository Suppliers => throw new NotImplementedException();
        public IPurchaseOrderRepository PurchaseOrders => throw new NotImplementedException();
        public IPurchaseInvoiceRepository PurchaseInvoices => throw new NotImplementedException();
        public IRepository<PurchaseInvoiceDueDate, Guid> PurchaseInvoiceDueDates => throw new NotImplementedException();
        public IRepository<InvoiceSerie, Guid> InvoiceSeries => throw new NotImplementedException();
        public IRepository<ExpenseType, Guid> ExpenseTypes => throw new NotImplementedException();
        public IExpenseRepository Expenses => throw new NotImplementedException();
        public IReceiptRepository Receipts => throw new NotImplementedException();
        public IRepository<ReferenceFormat, Guid> ReferenceFormats => throw new NotImplementedException();
        public IContractReader<ConsolidatedExpense> ConsolidatedExpenses => throw new NotImplementedException();
        public ITransportRateRepository TransportRates => throw new NotImplementedException();
        public IRepository<TransportRateDetail, Guid> TransportRateDetails => throw new NotImplementedException();
        public IPurchaseRateRepository PurchaseRates => throw new NotImplementedException();
        public IRepository<PurchaseRateDetail, Guid> PurchaseRateDetails => throw new NotImplementedException();
        public IRepository<CustomerType, Guid> CustomerTypes => throw new NotImplementedException();
        public IRepository<Reference, Guid> References => throw new NotImplementedException();
        public ISalesOrderHeaderRepository SalesOrderHeaders => throw new NotImplementedException();
        public ISalesOrderDetailRepository SalesOrderDetails => throw new NotImplementedException();
        public ISalesInvoiceRepository SalesInvoices => throw new NotImplementedException();
        public IRepository<SalesInvoiceVerifactuRequest, Guid> VerifactuRequests => throw new NotImplementedException();
        public IDeliveryNoteRepository DeliveryNotes => throw new NotImplementedException();
        public IBudgetRepository Budgets => throw new NotImplementedException();
        public IContractReader<ConsolidatedIncomes> ConsolidatedIncomes => throw new NotImplementedException();
        public IRepository<Enterprise, Guid> Enterprises => throw new NotImplementedException();
        public IRepository<Site, Guid> Sites => throw new NotImplementedException();
        public IAreaRepository Areas => throw new NotImplementedException();
        public IRepository<WorkcenterType, Guid> WorkcenterTypes => throw new NotImplementedException();
        public IWorkcenterRepository Workcenters => throw new NotImplementedException();
        public IRepository<WorkcenterCost, Guid> WorkcenterCosts => throw new NotImplementedException();
        public IRepository<Operator, Guid> Operators => throw new NotImplementedException();
        public IRepository<OperatorType, Guid> OperatorTypes => throw new NotImplementedException();
        public IMachineStatusRepository MachineStatuses => throw new NotImplementedException();
        public IRepository<Shift, Guid> Shifts => throw new NotImplementedException();
        public IRepository<ShiftDetail, Guid> ShiftDetails => throw new NotImplementedException();
        public IWorkMasterRepository WorkMasters => throw new NotImplementedException();
        public IWorkOrderRepository WorkOrders => throw new NotImplementedException();
        public IProductionPartRepository ProductionParts => throw new NotImplementedException();
        public IWorkcenterShiftRepository WorkcenterShifts => throw new NotImplementedException();
        public IContractReader<DetailedWorkOrder> DetailedWorkOrders => throw new NotImplementedException();
        public IContractReader<ProductionCost> ProductionCosts => throw new NotImplementedException();
        public IContractReader<WorkcenterShiftHistoricalOperator> WorkcenterShiftHistoricalOperators => throw new NotImplementedException();
        public IWorkcenterProfitPercentageRepository WorkcenterProfitPercentages => throw new NotImplementedException();
        public IPhaseTemplateRepository PhaseTemplates => throw new NotImplementedException();
        public IWarehouseRepository Warehouses => throw new NotImplementedException();
        public IRepository<WorkcenterLocation, Guid> WorkcenterLocations => throw new NotImplementedException();
        public IRepository<ReferenceType, Guid> ReferenceTypes => throw new NotImplementedException();
        public IRepository<Stock, Guid> Stocks => throw new NotImplementedException();
        public IStockMovementRepository StockMovements => throw new NotImplementedException();
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        private readonly List<Customer> _store;
        public List<Customer> Added { get; } = new();
        public List<Customer> Updated { get; } = new();

        public FakeCustomerRepository(IEnumerable<Customer> seed) => _store = seed.ToList();

        public IEnumerable<Customer> Find(Expression<Func<Customer, bool>> predicate)
            => _store.AsQueryable().Where(predicate).ToList();

        public Task<Customer?> Get(Guid id) => Task.FromResult(_store.FirstOrDefault(c => c.Id == id));

        public Task Add(Customer entity) { Added.Add(entity); _store.Add(entity); return Task.CompletedTask; }
        public Task Update(Customer entity) { Updated.Add(entity); return Task.CompletedTask; }

        // Unused members — surface accidental coupling.
        public Task AddWithoutSave(Customer entity) => throw new NotImplementedException();
        public Task AddRange(IEnumerable<Customer> entities) => throw new NotImplementedException();
        public Task AddRangeWithoutSave(IEnumerable<Customer> entities) => throw new NotImplementedException();
        public bool UpdateWithoutSave(Customer entity) => throw new NotImplementedException();
        public Task<bool> Exists(Guid id) => throw new NotImplementedException();
        public Task<List<Customer>> FindAsync(Expression<Func<Customer, bool>> predicate) => throw new NotImplementedException();
        public Task<List<Customer>> FindAsyncWithQueryParams(Expression<Func<Customer, bool>> predicate, Func<IQueryable<Customer>, IQueryable<Customer>>? includeFunc) => throw new NotImplementedException();
        public Task<IEnumerable<Customer>> GetAll() => throw new NotImplementedException();
        public Task Remove(Customer entity) => throw new NotImplementedException();
        public Task RemoveRange(IEnumerable<Customer> entities) => throw new NotImplementedException();
        public Task SaveChanges() => throw new NotImplementedException();

        public CustomerContact? GetContactById(Guid id) => throw new NotImplementedException();
        public Task AddContact(CustomerContact contact) => throw new NotImplementedException();
        public Task UpdateContact(CustomerContact contact) => throw new NotImplementedException();
        public Task RemoveContact(CustomerContact contact) => throw new NotImplementedException();
        public CustomerAddress? GetAddressById(Guid id) => throw new NotImplementedException();
        public Task AddAddress(CustomerAddress address) => throw new NotImplementedException();
        public Task UpdateAddress(CustomerAddress address) => throw new NotImplementedException();
        public Task RemoveAddress(CustomerAddress address) => throw new NotImplementedException();
    }

    private sealed class FakeLocalizationService : ILocalizationService
    {
        public int LookupCount { get; private set; }

        public string GetLocalizedString(string key, params object[] arguments)
        {
            LookupCount++;
            return key switch
            {
                "CustomerCifInvalid" => "CIF/NIF invàlid",
                "CustomerFiscalAddressInvalid" =>
                    "La direcció fiscal principal del client és incompleta. Cal omplir país, codi postal, ciutat i adreça",
                "CustomerInvalid" =>
                    "El client no és vàlid per a crear una factura. Revisa el nom fiscal, el número de compte i el NIF",
                "CustomerNoAddresses" =>
                    "El client no té direccions donades d'alta. Si us plau, crei una direcció.",
                "CustomerAlreadyExists" => $"Client {arguments[0]} existent",
                "EntityNotFound" => $"L'entitat amb ID {arguments[0]} no existeix",
                _ => key,
            };
        }

        public string GetLocalizedStringForCulture(string key, string culture, params object[] arguments)
            => GetLocalizedString(key, arguments);
        public Dictionary<string, string> GetAllTranslations() => new();
        public Dictionary<string, string> GetAllTranslationsForCulture(string culture) => new();
        public string[] GetSupportedCultures() => Array.Empty<string>();
    }

    private sealed class FakeGeolocalizationService : IGeolocalizationService
    {
        public Task<Coordinates?> GetCoordinatesAsync(string address, string city, string postalCode, string country)
            => Task.FromResult<Coordinates?>(null);
        public Task<decimal?> GetDistanceAsync(Coordinates origin, Coordinates destination)
            => Task.FromResult<decimal?>(null);
    }
}
