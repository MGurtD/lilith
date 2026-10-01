using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestrictMasterDataForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Sites_SiteId",
                table: "Areas");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetDetails_References_ReferenceId",
                table: "BudgetDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Customers_CustomerId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Exercises_ExerciseId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_CustomerTypes_CustomerTypeId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNoteDetails_References_ReferenceId",
                table: "DeliveryNoteDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_Customers_CustomerId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_Exercises_ExerciseId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_Sites_SiteId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_Statuses_StatusId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_ExpenseTypes_ExpenseTypeId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Operators_OperatorTypes_OperatorTypeId",
                table: "Operators");

            migrationBuilder.DropForeignKey(
                name: "FK_PhaseTemplateDetail_MachineStatuses_MachineStatusId",
                table: "PhaseTemplateDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionParts_Workcenters_WorkcenterId",
                table: "ProductionParts");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceImports_Taxes_TaxId",
                table: "PurchaseInvoiceImports");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PaymentMethods_PaymentMethodId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Suppliers_SupplierId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderDetails_References_ReferenceId",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderDetails_Statuses_StatusId",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Exercises_ExerciseId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Statuses_StatusId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptDetails_References_ReferenceId",
                table: "ReceiptDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Exercises_ExerciseId",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Statuses_StatusId",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Suppliers_SupplierId",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoice_PaymentMethods_PaymentMethodId",
                table: "SalesInvoice");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceDetails_Taxes_TaxId",
                table: "SalesInvoiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceImports_Taxes_TaxId",
                table: "SalesInvoiceImports");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetail_References_ReferenceId",
                table: "SalesOrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Enterprises_EnterpriseId",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_References_ReferenceId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Locations_LocationId",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_References_ReferenceId",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_SupplierTypes_SupplierTypeId",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Sites_SiteId",
                table: "Warehouses");

            migrationBuilder.DropForeignKey(
                name: "FK_Workcenters_Areas_AreaId",
                table: "Workcenters");

            migrationBuilder.DropForeignKey(
                name: "FK_Workcenters_WorkcenterTypes_WorkcenterTypeId",
                table: "Workcenters");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkcenterShiftDetails_MachineStatuses_MachineStatusId",
                schema: "data",
                table: "WorkcenterShiftDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkcenterShifts_Workcenters_WorkcenterId",
                schema: "data",
                table: "WorkcenterShifts");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMaster_References_ReferenceId",
                table: "WorkMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseBillOfMaterials_References_ReferenceId",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_Exercises_ExerciseId",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_References_ReferenceId",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_Statuses_StatusId",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_WorkMaster_WorkMasterId",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhase_Statuses_StatusId",
                table: "WorkOrderPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhaseBillOfMaterials_References_ReferenceId",
                table: "WorkOrderPhaseBillOfMaterials");

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Sites_SiteId",
                table: "Areas",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetDetails_References_ReferenceId",
                table: "BudgetDetails",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Customers_CustomerId",
                table: "Budgets",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Exercises_ExerciseId",
                table: "Budgets",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_CustomerTypes_CustomerTypeId",
                table: "Customers",
                column: "CustomerTypeId",
                principalTable: "CustomerTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNoteDetails_References_ReferenceId",
                table: "DeliveryNoteDetails",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_Customers_CustomerId",
                table: "DeliveryNotes",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_Exercises_ExerciseId",
                table: "DeliveryNotes",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_Sites_SiteId",
                table: "DeliveryNotes",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_Statuses_StatusId",
                table: "DeliveryNotes",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_ExpenseTypes_ExpenseTypeId",
                table: "Expenses",
                column: "ExpenseTypeId",
                principalTable: "ExpenseTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Operators_OperatorTypes_OperatorTypeId",
                table: "Operators",
                column: "OperatorTypeId",
                principalTable: "OperatorTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PhaseTemplateDetail_MachineStatuses_MachineStatusId",
                table: "PhaseTemplateDetail",
                column: "MachineStatusId",
                principalTable: "MachineStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionParts_Workcenters_WorkcenterId",
                table: "ProductionParts",
                column: "WorkcenterId",
                principalTable: "Workcenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceImports_Taxes_TaxId",
                table: "PurchaseInvoiceImports",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PaymentMethods_PaymentMethodId",
                table: "PurchaseInvoices",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Suppliers_SupplierId",
                table: "PurchaseInvoices",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderDetails_References_ReferenceId",
                table: "PurchaseOrderDetails",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderDetails_Statuses_StatusId",
                table: "PurchaseOrderDetails",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Exercises_ExerciseId",
                table: "PurchaseOrders",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Statuses_StatusId",
                table: "PurchaseOrders",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptDetails_References_ReferenceId",
                table: "ReceiptDetails",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Exercises_ExerciseId",
                table: "Receipts",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Statuses_StatusId",
                table: "Receipts",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Suppliers_SupplierId",
                table: "Receipts",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoice_PaymentMethods_PaymentMethodId",
                table: "SalesInvoice",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceDetails_Taxes_TaxId",
                table: "SalesInvoiceDetails",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceImports_Taxes_TaxId",
                table: "SalesInvoiceImports",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetail_References_ReferenceId",
                table: "SalesOrderDetail",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Enterprises_EnterpriseId",
                table: "Sites",
                column: "EnterpriseId",
                principalTable: "Enterprises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_References_ReferenceId",
                table: "StockMovements",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Locations_LocationId",
                table: "Stocks",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_References_ReferenceId",
                table: "Stocks",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_SupplierTypes_SupplierTypeId",
                table: "Suppliers",
                column: "SupplierTypeId",
                principalTable: "SupplierTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Sites_SiteId",
                table: "Warehouses",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workcenters_Areas_AreaId",
                table: "Workcenters",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workcenters_WorkcenterTypes_WorkcenterTypeId",
                table: "Workcenters",
                column: "WorkcenterTypeId",
                principalTable: "WorkcenterTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkcenterShiftDetails_MachineStatuses_MachineStatusId",
                schema: "data",
                table: "WorkcenterShiftDetails",
                column: "MachineStatusId",
                principalTable: "MachineStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkcenterShifts_Workcenters_WorkcenterId",
                schema: "data",
                table: "WorkcenterShifts",
                column: "WorkcenterId",
                principalTable: "Workcenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMaster_References_ReferenceId",
                table: "WorkMaster",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseBillOfMaterials_References_ReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail",
                column: "MachineStatusId",
                principalTable: "MachineStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_Exercises_ExerciseId",
                table: "WorkOrder",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_References_ReferenceId",
                table: "WorkOrder",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_Statuses_StatusId",
                table: "WorkOrder",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_WorkMaster_WorkMasterId",
                table: "WorkOrder",
                column: "WorkMasterId",
                principalTable: "WorkMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhase_Statuses_StatusId",
                table: "WorkOrderPhase",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhaseBillOfMaterials_References_ReferenceId",
                table: "WorkOrderPhaseBillOfMaterials",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Sites_SiteId",
                table: "Areas");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetDetails_References_ReferenceId",
                table: "BudgetDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Customers_CustomerId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Exercises_ExerciseId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_CustomerTypes_CustomerTypeId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNoteDetails_References_ReferenceId",
                table: "DeliveryNoteDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_Customers_CustomerId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_Exercises_ExerciseId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_Sites_SiteId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_Statuses_StatusId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_ExpenseTypes_ExpenseTypeId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Operators_OperatorTypes_OperatorTypeId",
                table: "Operators");

            migrationBuilder.DropForeignKey(
                name: "FK_PhaseTemplateDetail_MachineStatuses_MachineStatusId",
                table: "PhaseTemplateDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionParts_Workcenters_WorkcenterId",
                table: "ProductionParts");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceImports_Taxes_TaxId",
                table: "PurchaseInvoiceImports");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PaymentMethods_PaymentMethodId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Suppliers_SupplierId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderDetails_References_ReferenceId",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderDetails_Statuses_StatusId",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Exercises_ExerciseId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Statuses_StatusId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptDetails_References_ReferenceId",
                table: "ReceiptDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Exercises_ExerciseId",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Statuses_StatusId",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Suppliers_SupplierId",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoice_PaymentMethods_PaymentMethodId",
                table: "SalesInvoice");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceDetails_Taxes_TaxId",
                table: "SalesInvoiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceImports_Taxes_TaxId",
                table: "SalesInvoiceImports");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetail_References_ReferenceId",
                table: "SalesOrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Enterprises_EnterpriseId",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_References_ReferenceId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Locations_LocationId",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_References_ReferenceId",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_SupplierTypes_SupplierTypeId",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Sites_SiteId",
                table: "Warehouses");

            migrationBuilder.DropForeignKey(
                name: "FK_Workcenters_Areas_AreaId",
                table: "Workcenters");

            migrationBuilder.DropForeignKey(
                name: "FK_Workcenters_WorkcenterTypes_WorkcenterTypeId",
                table: "Workcenters");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkcenterShiftDetails_MachineStatuses_MachineStatusId",
                schema: "data",
                table: "WorkcenterShiftDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkcenterShifts_Workcenters_WorkcenterId",
                schema: "data",
                table: "WorkcenterShifts");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMaster_References_ReferenceId",
                table: "WorkMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseBillOfMaterials_References_ReferenceId",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_Exercises_ExerciseId",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_References_ReferenceId",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_Statuses_StatusId",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_WorkMaster_WorkMasterId",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhase_Statuses_StatusId",
                table: "WorkOrderPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhaseBillOfMaterials_References_ReferenceId",
                table: "WorkOrderPhaseBillOfMaterials");

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Sites_SiteId",
                table: "Areas",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetDetails_References_ReferenceId",
                table: "BudgetDetails",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Customers_CustomerId",
                table: "Budgets",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Exercises_ExerciseId",
                table: "Budgets",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_CustomerTypes_CustomerTypeId",
                table: "Customers",
                column: "CustomerTypeId",
                principalTable: "CustomerTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNoteDetails_References_ReferenceId",
                table: "DeliveryNoteDetails",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_Customers_CustomerId",
                table: "DeliveryNotes",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_Exercises_ExerciseId",
                table: "DeliveryNotes",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_Sites_SiteId",
                table: "DeliveryNotes",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_Statuses_StatusId",
                table: "DeliveryNotes",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_ExpenseTypes_ExpenseTypeId",
                table: "Expenses",
                column: "ExpenseTypeId",
                principalTable: "ExpenseTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Operators_OperatorTypes_OperatorTypeId",
                table: "Operators",
                column: "OperatorTypeId",
                principalTable: "OperatorTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhaseTemplateDetail_MachineStatuses_MachineStatusId",
                table: "PhaseTemplateDetail",
                column: "MachineStatusId",
                principalTable: "MachineStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionParts_Workcenters_WorkcenterId",
                table: "ProductionParts",
                column: "WorkcenterId",
                principalTable: "Workcenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceImports_Taxes_TaxId",
                table: "PurchaseInvoiceImports",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PaymentMethods_PaymentMethodId",
                table: "PurchaseInvoices",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Suppliers_SupplierId",
                table: "PurchaseInvoices",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderDetails_References_ReferenceId",
                table: "PurchaseOrderDetails",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderDetails_Statuses_StatusId",
                table: "PurchaseOrderDetails",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Exercises_ExerciseId",
                table: "PurchaseOrders",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Statuses_StatusId",
                table: "PurchaseOrders",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptDetails_References_ReferenceId",
                table: "ReceiptDetails",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Exercises_ExerciseId",
                table: "Receipts",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Statuses_StatusId",
                table: "Receipts",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Suppliers_SupplierId",
                table: "Receipts",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoice_PaymentMethods_PaymentMethodId",
                table: "SalesInvoice",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceDetails_Taxes_TaxId",
                table: "SalesInvoiceDetails",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceImports_Taxes_TaxId",
                table: "SalesInvoiceImports",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetail_References_ReferenceId",
                table: "SalesOrderDetail",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Enterprises_EnterpriseId",
                table: "Sites",
                column: "EnterpriseId",
                principalTable: "Enterprises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_References_ReferenceId",
                table: "StockMovements",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Locations_LocationId",
                table: "Stocks",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_References_ReferenceId",
                table: "Stocks",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_SupplierTypes_SupplierTypeId",
                table: "Suppliers",
                column: "SupplierTypeId",
                principalTable: "SupplierTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Sites_SiteId",
                table: "Warehouses",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workcenters_Areas_AreaId",
                table: "Workcenters",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workcenters_WorkcenterTypes_WorkcenterTypeId",
                table: "Workcenters",
                column: "WorkcenterTypeId",
                principalTable: "WorkcenterTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkcenterShiftDetails_MachineStatuses_MachineStatusId",
                schema: "data",
                table: "WorkcenterShiftDetails",
                column: "MachineStatusId",
                principalTable: "MachineStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkcenterShifts_Workcenters_WorkcenterId",
                schema: "data",
                table: "WorkcenterShifts",
                column: "WorkcenterId",
                principalTable: "Workcenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMaster_References_ReferenceId",
                table: "WorkMaster",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseBillOfMaterials_References_ReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail",
                column: "MachineStatusId",
                principalTable: "MachineStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_Exercises_ExerciseId",
                table: "WorkOrder",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_References_ReferenceId",
                table: "WorkOrder",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_Statuses_StatusId",
                table: "WorkOrder",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_WorkMaster_WorkMasterId",
                table: "WorkOrder",
                column: "WorkMasterId",
                principalTable: "WorkMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhase_Statuses_StatusId",
                table: "WorkOrderPhase",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhaseBillOfMaterials_References_ReferenceId",
                table: "WorkOrderPhaseBillOfMaterials",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
