using System.Linq.Expressions;
using Application.Contracts;
using Application.Contracts.Persistance.Repositories.Purchase;
using Application.Services;
using Application.Services.System;
using Domain.Entities;
using Domain.Entities.Auth;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Domain.Entities.Transport;
using Domain.Entities.Warehouse;
using Xunit;
using System.Data;

namespace Application.Tests.Services.Shared;

public class ExerciseServiceTests
{
    [Theory]
    [InlineData("purchaseorder")]
    [InlineData("purchaseinvoice")]
    [InlineData("salesinvoice")]
    [InlineData("salesorder")]
    [InlineData("receipt")]
    [InlineData("deliverynote")]
    [InlineData("budget")]
    [InlineData("workorder")]
    public async Task GetNextCounter_preserves_sequences_beyond_three_digits(string counterName)
    {
        var exercise = new Exercise { Id = Guid.NewGuid(), Name = "2026" };
        SetCounter(exercise, counterName, "998");
        var service = new ExerciseService(new TestUnitOfWork(exercise), new TestLocalizationService());

        var first = await service.GetNextCounter(exercise.Id, counterName);
        var second = await service.GetNextCounter(exercise.Id, counterName);
        var third = await service.GetNextCounter(exercise.Id, counterName);

        Assert.Equal("26999", first.Content);
        Assert.Equal("261000", second.Content);
        Assert.Equal("261001", third.Content);
        Assert.Equal("1001", GetCounter(exercise, counterName));
    }

    private static void SetCounter(Exercise exercise, string counterName, string value)
        => GetCounterProperty(exercise, counterName).SetValue(exercise, value);

    private static string GetCounter(Exercise exercise, string counterName)
        => (string)GetCounterProperty(exercise, counterName).GetValue(exercise)!;

    private static System.Reflection.PropertyInfo GetCounterProperty(Exercise exercise, string counterName)
        => typeof(Exercise).GetProperty(counterName switch
        {
            "purchaseorder" => nameof(Exercise.PurchaseOrderCounter),
            "purchaseinvoice" => nameof(Exercise.PurchaseInvoiceCounter),
            "salesinvoice" => nameof(Exercise.SalesInvoiceCounter),
            "salesorder" => nameof(Exercise.SalesOrderCounter),
            "receipt" => nameof(Exercise.ReceiptCounter),
            "deliverynote" => nameof(Exercise.DeliveryNoteCounter),
            "budget" => nameof(Exercise.BudgetCounter),
            "workorder" => nameof(Exercise.WorkOrderCounter),
            _ => throw new InvalidOperationException($"Counter not found: {counterName}")
        })!;

    private sealed class TestUnitOfWork(Exercise exercise) : IUnitOfWork
    {
        public IRepository<Exercise, Guid> Exercices { get; } = new ExerciseRepository(exercise);

        public Task<IUnitOfWorkTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) => throw new NotImplementedException();
        public Task<int> CompleteAsync() => Task.FromResult(0);
        public void Dispose() { }

        private sealed class ExerciseRepository(Exercise exercise) : IRepository<Exercise, Guid>
        {
            public Task<Exercise?> Get(Guid id) => Task.FromResult<Exercise?>(exercise.Id == id ? exercise : null);
            public Task Update(Exercise entity) => Task.CompletedTask;
            public Task<IEnumerable<Exercise>> GetAll() => throw new NotImplementedException();
            public Task<List<Exercise>> FindAsync(Expression<Func<Exercise, bool>> predicate) => throw new NotImplementedException();
            public Task<List<Exercise>> FindAsyncWithQueryParams(Expression<Func<Exercise, bool>> predicate, Func<IQueryable<Exercise>, IQueryable<Exercise>>? includeFunc) => throw new NotImplementedException();
            public IEnumerable<Exercise> Find(Expression<Func<Exercise, bool>> predicate) => throw new NotImplementedException();
            public Task<bool> Exists(Guid id) => throw new NotImplementedException();
            public Task Add(Exercise entity) => throw new NotImplementedException();
            public Task AddWithoutSave(Exercise entity) => throw new NotImplementedException();
            public Task AddRange(IEnumerable<Exercise> entities) => throw new NotImplementedException();
            public Task AddRangeWithoutSave(IEnumerable<Exercise> entities) => throw new NotImplementedException();
            public bool UpdateWithoutSave(Exercise entity) => throw new NotImplementedException();
            public Task Remove(Exercise entity) => throw new NotImplementedException();
            public Task RemoveRange(IEnumerable<Exercise> entities) => throw new NotImplementedException();
            public Task SaveChanges() => throw new NotImplementedException();
        }

        public IRepository<Role, Guid> Roles => throw new NotImplementedException();
        public IRepository<User, Guid> Users => throw new NotImplementedException();
        public IRepository<ApiKey, Guid> ApiKeys => throw new NotImplementedException();
        public IRepository<UserRefreshToken, Guid> UserRefreshTokens => throw new NotImplementedException();
        public IRepository<UserFilter, Guid> UserFilters => throw new NotImplementedException();
        public IRepository<UserTableView, Guid> UserTableViews => throw new NotImplementedException();
        public IRepository<Profile, Guid> Profiles => throw new NotImplementedException();
        public IRepository<MenuItem, Guid> MenuItems => throw new NotImplementedException();
        public IRepository<ProfileMenuItem, Guid> ProfileMenuItems => throw new NotImplementedException();
        public IRepository<Domain.Entities.File, Guid> Files => throw new NotImplementedException();
        public IRepository<Parameter, Guid> Parameters => throw new NotImplementedException();
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
        public ICustomerRepository Customers => throw new NotImplementedException();
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

    private sealed class TestLocalizationService : ILocalizationService
    {
        public string GetLocalizedString(string key, params object[] arguments) => key;
        public string GetLocalizedStringForCulture(string key, string culture, params object[] arguments) => key;
        public Dictionary<string, string> GetAllTranslations() => new();
        public Dictionary<string, string> GetAllTranslationsForCulture(string culture) => new();
        public string[] GetSupportedCultures() => Array.Empty<string>();
    }
}
