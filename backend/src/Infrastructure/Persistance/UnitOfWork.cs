using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Auth;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Domain.Entities.Warehouse;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Persistance.Repositories.Production;
using Infrastructure.Persistance.Repositories.Purchase; // contains LifecycleRepository as per current project structure
using Infrastructure.Persistance.Repositories.Sales;
using Infrastructure.Persistance.Repositories.Warehouse;

namespace Infrastructure.Persistance
{
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork, IDisposable
    {
        public readonly ApplicationDbContext context = context;

        // Authentication
        public IRepository<Role, Guid> Roles { get; private set; } = new Repository<Role, Guid>(context);
        public IRepository<User, Guid> Users { get; private set; } = new Repository<User, Guid>(context);
        public IRepository<UserRefreshToken, Guid> UserRefreshTokens { get; private set; } = new Repository<UserRefreshToken, Guid>(context);
        public IRepository<UserFilter, Guid> UserFilters { get; private set; } = new Repository<UserFilter, Guid>(context);
        public IRepository<Profile, Guid> Profiles { get; private set; } = new Repository<Profile, Guid>(context);
        public IRepository<MenuItem, Guid> MenuItems { get; private set; } = new Repository<MenuItem, Guid>(context);
        public IRepository<ProfileMenuItem, Guid> ProfileMenuItems { get; private set; } = new Repository<ProfileMenuItem, Guid>(context);

        // Shared
        public IRepository<Domain.Entities.File, Guid> Files { get; private set; } = new Repository<Domain.Entities.File, Guid>(context);
        public IRepository<Parameter, Guid> Parameters { get; private set; } = new Repository<Parameter, Guid>(context);
        public IRepository<Exercise, Guid> Exercices { get; private set; } = new Repository<Exercise, Guid>(context);
        public IRepository<Tax, Guid> Taxes { get; private set; } = new Repository<Tax, Guid>(context);
        public IRepository<PaymentMethod, Guid> PaymentMethods { get; private set; } = new Repository<PaymentMethod, Guid>(context);
        public ILifecycleRepository Lifecycles { get; private set; } = new LifecycleRepository(context);
        public ILifecycleTagRepository LifecycleTags { get; private set; } = new LifecycleTagRepository(context);
        public IRepository<StatusLifecycleTag, Guid> StatusLifecycleTags { get; private set; } = new Repository<StatusLifecycleTag, Guid>(context);

        // Purchase
        public IRepository<SupplierType, Guid> SupplierTypes { get; private set; } = new Repository<SupplierType, Guid>(context);
        public ISupplierRepository Suppliers { get; private set; } = new SupplierRepository(context);
        public IPurchaseOrderRepository PurchaseOrders { get; private set; } = new PurchaseOrderRepository(context);
        public IPurchaseInvoiceRepository PurchaseInvoices { get; private set; } = new PurchaseInvoiceRepository(context);
        public IRepository<PurchaseInvoiceDueDate, Guid> PurchaseInvoiceDueDates { get; private set; } = new Repository<PurchaseInvoiceDueDate, Guid>(context);
        public IRepository<InvoiceSerie, Guid> InvoiceSeries { get; private set; } = new Repository<InvoiceSerie, Guid>(context);
        public IRepository<ExpenseType, Guid> ExpenseTypes { get; private set; } = new Repository<ExpenseType, Guid>(context);
        public IExpenseRepository Expenses { get; private set; } = new ExpenseRepository(context);
        public IRepository<ReferenceFormat, Guid> ReferenceFormats { get; private set; } = new Repository<ReferenceFormat, Guid>(context);
        public IContractReader<ConsolidatedExpense> ConsolidatedExpenses { get; private set; } = new ContractReader<ConsolidatedExpense>(context);

        // Sales
        public IRepository<CustomerType, Guid> CustomerTypes { get; private set; } = new Repository<CustomerType, Guid>(context);
        public ICustomerRepository Customers { get; private set; } = new CustomerRepository(context);
        public IRepository<Reference, Guid> References { get; private set; } = new Repository<Reference, Guid>(context);
        public ISalesOrderHeaderRepository SalesOrderHeaders { get; private set; } = new SalesOrderHeaderRepository(context, new SalesOrderDetailRepository(context));
        public ISalesOrderDetailRepository SalesOrderDetails { get; private set; } = new SalesOrderDetailRepository(context);
        public ISalesInvoiceRepository SalesInvoices { get; private set; } = new SalesInvoiceRepository(context);
        public IRepository<SalesInvoiceVerifactuRequest, Guid> VerifactuRequests { get; private set; } = new Repository<SalesInvoiceVerifactuRequest, Guid>(context);
        public IReceiptRepository Receipts { get; private set; } = new ReceiptRepository(context);
        public IDeliveryNoteRepository DeliveryNotes { get; private set; } = new DeliveryNoteRepository(context);
        public IBudgetRepository Budgets { get; private set; } = new BudgetRepository(context);
        public IContractReader<ConsolidatedIncomes> ConsolidatedIncomes { get; private set; } = new ContractReader<ConsolidatedIncomes>(context);

        // Production
        public IRepository<Enterprise, Guid> Enterprises { get; private set; } = new Repository<Enterprise, Guid>(context);
        public IRepository<Site, Guid> Sites { get; private set; } = new Repository<Site, Guid>(context);
        public IAreaRepository Areas { get; private set; } = new AreaRepository(context);
        public IRepository<WorkcenterType, Guid> WorkcenterTypes { get; private set; } = new Repository<WorkcenterType, Guid>(context);
        public IWorkcenterRepository Workcenters { get; private set; } = new WorkcenterRepository(context);
        public IRepository<WorkcenterCost, Guid> WorkcenterCosts { get; private set; } = new Repository<WorkcenterCost, Guid>(context);
        public IRepository<Operator, Guid> Operators { get; private set; } = new Repository<Operator, Guid>(context);
        public IRepository<OperatorType, Guid> OperatorTypes { get; private set; } = new Repository<OperatorType, Guid>(context);
        public IMachineStatusRepository MachineStatuses { get; private set; } = new MachineStatusRepository(context);
        public IRepository<Shift, Guid> Shifts { get; private set; } = new Repository<Shift, Guid>(context);
        public IRepository<ShiftDetail, Guid> ShiftDetails { get; private set; } = new Repository<ShiftDetail, Guid>(context);
        public IWorkMasterRepository WorkMasters { get; private set; } = new WorkMasterRepository(context);
        public IWorkOrderRepository WorkOrders { get; private set; } = new WorkOrderRepository(context);
        public IProductionPartRepository ProductionParts { get; private set; } = new ProductionPartRepository(context);
        public IWorkcenterShiftRepository WorkcenterShifts { get; private set; } = new WorkcenterShiftRepository(context);
        public IContractReader<DetailedWorkOrder> DetailedWorkOrders { get; private set; } = new ContractReader<DetailedWorkOrder>(context);
        public IContractReader<ProductionCost> ProductionCosts { get; private set; } = new ContractReader<ProductionCost>(context);
        public IContractReader<WorkcenterShiftHistoricalOperator> WorkcenterShiftHistoricalOperators { get; private set; } = new ContractReader<WorkcenterShiftHistoricalOperator>(context);
        public IWorkcenterProfitPercentageRepository WorkcenterProfitPercentages { get; private set; } = new WorkcenterProfitPercentageRepository(context); 

        // Warehouse
        public IWarehouseRepository Warehouses { get; private set; } = new WarehouseRepository(context);
        public IRepository<WorkcenterLocation, Guid> WorkcenterLocations { get; private set; } = new Repository<WorkcenterLocation, Guid>(context);
        public IRepository<ReferenceType, Guid> ReferenceTypes { get; private set; } = new Repository<ReferenceType, Guid>(context);
        public IRepository<Stock, Guid> Stocks { get; private set; } = new Repository<Stock, Guid>(context);
        public IRepository<StockMovement, Guid> StockMovements { get; private set; } = new Repository<StockMovement, Guid>(context);

        public async Task<int> CompleteAsync()
        {
            return await context.SaveChangesAsync();
        }

        public void Dispose() => context.Dispose();
    }
}

