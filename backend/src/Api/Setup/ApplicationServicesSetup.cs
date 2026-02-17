using Application.Services.System;
using Application.Services.Production;
using Application.Services.Purchase;
using Application.Services.Sales;
using Application.Services.Shared;
using Application.Services.Verifactu;
using Application.Services.Warehouse;
using Application.Contracts;
using Infrastructure.Persistance;

namespace Api.Setup;

public static class ApplicationServicesSetup
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IQrCodeService, QrCodeService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<IDueDateService, DueDateService>();
        services.AddScoped<IReferenceService, ReferenceService>();
        services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IDeliveryNoteService, DeliveryNoteService>();
        services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
        services.AddScoped<ICustomerRankingService, CustomerRankingService>();
        services.AddScoped<ISalesInvoiceReportService, SalesInvoiceReportService>();
        services.AddScoped<ISalesOrderReportService, SalesOrderReportService>();
        services.AddScoped<IBudgetReportService, BudgetReportService>();
        services.AddScoped<IDeliveryNoteReportService, DeliveryNoteReportService>();
        services.AddScoped<IPurchaseOrderReportService, PurchaseOrderReportService>();
        services.AddScoped<ISalesOrderReportService, SalesOrderReportService>();
        services.AddScoped<IEnterpriseService, EnterpriseService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<IWorkOrderReportService, WorkOrderReportService>();
        services.AddScoped<IMetricsService, MetricsService>();
        services.AddScoped<IWorkcenterShiftService, WorkcenterShiftService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IStockMovementService, StockMovementService>();
        services.AddScoped<IVerifactuIntegrationService, VerifactuIntegrationService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IMenuItemService, MenuItemService>();
        services.AddScoped<ILifecycleService, LifecycleService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IReferenceTypeService, ReferenceTypeService>();
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserFilterService, UserFilterService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerTypeService, CustomerTypeService>();
        services.AddScoped<IIncomeService, IncomeService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ISupplierTypeService, SupplierTypeService>();
        services.AddScoped<IExpenseTypeService, ExpenseTypeService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IInvoiceSerieService, InvoiceSerieService>();
        
        // Production services - Group A Simple CRUD
        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<IAreaService, AreaService>();
        services.AddScoped<IWorkcenterTypeService, WorkcenterTypeService>();
        services.AddScoped<IOperatorTypeService, OperatorTypeService>();
        services.AddScoped<IMachineStatusService, MachineStatusService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IShiftDetailService, ShiftDetailService>();
        services.AddScoped<IWorkcenterCostService, WorkcenterCostService>();
        services.AddScoped<IWorkcenterProfitPercentageService, WorkcenterProfitPercentageService>();

        // Production services - Group B Extend existing
        services.AddScoped<IWorkMasterService, WorkMasterService>();
        services.AddScoped<IProductionPartService, ProductionPartService>();
        
        // Production services - Group C Specialized
        services.AddScoped<IWorkcenterService, WorkcenterService>();
        services.AddScoped<IOperatorService, OperatorService>();
        services.AddScoped<IProductionCostService, ProductionCostService>();
        services.AddScoped<IWorkMasterPhaseService, WorkMasterPhaseService>();
        services.AddScoped<IWorkOrderPhaseService, WorkOrderPhaseService>();
        services.AddScoped<IDetailedWorkOrderService, DetailedWorkOrderService>();
        
        // Production services - Background jobs
        services.AddSingleton<IProductionPartChannel, ProductionPartChannel>();
        services.AddScoped<IProductionPartGeneratorHandler, ProductionPartGeneratorHandler>();
        services.AddHostedService<ProductionPartGeneratorService>();
        
        // Warehouse services
        services.AddScoped<IWarehouseService, WarehouseService>();
        
        services.AddHostedService<BudgetBackgroundService>();

        return services;
    }
}
