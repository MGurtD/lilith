using Application.Contracts;
using Application.Contracts.Persistance.Repositories.Purchase;
using Domain.Entities;
using Domain.Entities.Auth;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Domain.Entities.Warehouse;
using Domain.Entities.Transport;
using System.Data;

namespace Application.Contracts
{
    public interface IUnitOfWork
    {
        // Authentication
        IRepository<Role, Guid> Roles { get; }
        IRepository<User, Guid> Users { get; }
        IRepository<ApiKey, Guid> ApiKeys { get; }
        IRepository<UserRefreshToken, Guid> UserRefreshTokens { get; }
        IRepository<UserFilter, Guid> UserFilters { get; }
        IRepository<UserTableView, Guid> UserTableViews { get; }
        IRepository<Profile, Guid> Profiles { get; }
        IRepository<MenuItem, Guid> MenuItems { get; }
        IRepository<MenuItemTranslation, Guid> MenuItemTranslations { get; }
        IRepository<ProfileMenuItem, Guid> ProfileMenuItems { get; }

        // Shared
        IRepository<Domain.Entities.File, Guid> Files { get; }
        IRepository<Parameter, Guid> Parameters { get; }
        IExerciseRepository Exercices { get; }
        ITaxRepository Taxes { get; }
        IPaymentMethodRepository PaymentMethods { get; }
        ILifecycleRepository Lifecycles { get; }
        ILifecycleTagRepository LifecycleTags { get; }
        IRepository<StatusLifecycleTag, Guid> StatusLifecycleTags { get; }

        // Purchase
        ISupplierTypeRepository SupplierTypes { get; }
        ISupplierRepository Suppliers { get; }
        IPurchaseOrderRepository PurchaseOrders { get; }
        IPurchaseInvoiceRepository PurchaseInvoices { get; }
        IRepository<PurchaseInvoiceDueDate, Guid> PurchaseInvoiceDueDates { get; }
        IRepository<InvoiceSerie, Guid> InvoiceSeries { get; }
        IExpenseTypeRepository ExpenseTypes { get; }
        IExpenseRepository Expenses { get; }
        IReceiptRepository Receipts { get; }
        IRepository<ReferenceFormat, Guid> ReferenceFormats { get; }
        IContractReader<ConsolidatedExpense> ConsolidatedExpenses { get; }
        ITransportRateRepository TransportRates { get; }
        IRepository<TransportRateDetail, Guid> TransportRateDetails { get; }
        IPurchaseRateRepository PurchaseRates { get; }
        IRepository<PurchaseRateDetail, Guid> PurchaseRateDetails { get; }

        // Sales
        ICustomerTypeRepository CustomerTypes { get; }
        ICustomerRepository Customers { get; }
        IReferenceRepository References { get; }
        ISalesOrderHeaderRepository SalesOrderHeaders { get; }
        ISalesOrderDetailRepository SalesOrderDetails { get; }
        ISalesInvoiceRepository SalesInvoices { get; }
        IRepository<SalesInvoiceVerifactuRequest, Guid> VerifactuRequests { get; }
        IDeliveryNoteRepository DeliveryNotes { get; }
        IBudgetRepository Budgets { get; }
        IContractReader<ConsolidatedIncomes> ConsolidatedIncomes { get; }

        // Production
        IEnterpriseRepository Enterprises { get; }
        ISiteRepository Sites { get; }
        IAreaRepository Areas { get; }
        IWorkcenterTypeRepository WorkcenterTypes { get; }
        IWorkcenterRepository Workcenters { get; }
        IRepository<WorkcenterCost, Guid> WorkcenterCosts { get; }
        IRepository<Operator, Guid> Operators { get; }
        IOperatorTypeRepository OperatorTypes { get; }
        IRepository<RejectionReason, Guid> RejectionReasons { get; }
        IRepository<WorkOrderPhaseRejection, Guid> WorkOrderPhaseRejections { get; }
        IMachineStatusRepository MachineStatuses { get; }
        IRepository<Shift, Guid> Shifts { get; }
        IRepository<ShiftDetail, Guid> ShiftDetails { get; }
        IWorkMasterRepository WorkMasters { get; }
        IWorkOrderRepository WorkOrders { get; }
        IProductionPartRepository ProductionParts { get; }
        IWorkcenterShiftRepository WorkcenterShifts { get; }
        IContractReader<DetailedWorkOrder> DetailedWorkOrders { get; }
        IContractReader<ProductionCost> ProductionCosts { get; }
        IContractReader<WorkcenterShiftHistoricalOperator> WorkcenterShiftHistoricalOperators { get; }
        IWorkcenterProfitPercentageRepository WorkcenterProfitPercentages { get; }
        IPhaseTemplateRepository PhaseTemplates { get; }

        //Warehouse
        IWarehouseRepository Warehouses { get; }
        IRepository<WorkcenterLocation, Guid> WorkcenterLocations { get; }
        IRepository<ReferenceType, Guid> ReferenceTypes { get; }
        IRepository<Stock, Guid> Stocks { get; }
        ILotRepository Lots { get; }
        IStockMovementRepository StockMovements { get; }

        Task<IUnitOfWorkTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        Task<int> CompleteAsync();
        void Dispose();
    }
}
