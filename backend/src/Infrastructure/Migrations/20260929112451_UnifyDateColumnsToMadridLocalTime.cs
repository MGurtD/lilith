using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// Every date is stored as "timestamp without time zone" in Europe/Madrid wall-clock
    /// time, except 18 columns that earlier migrations created as timestamptz (before the
    /// design-time factory enabled Npgsql's legacy timestamp behaviour). This converts them
    /// so the whole schema follows one convention.
    ///
    /// Hand-written instead of the scaffolded AlterColumn operations because:
    /// - "AT TIME ZONE 'Europe/Madrid'" keeps the exact local date and time users see
    ///   today; a plain cast would use the session time zone and shift the values.
    /// - PostgreSQL cannot change the type of a column that a view uses, so the three
    ///   dependent views are dropped and created again with their current definition.
    /// </summary>
    public partial class UnifyDateColumnsToMadridLocalTime : Migration
    {
        private static readonly (string Table, string Column)[] Columns =
        [
            ("Lot", "ClosedDate"),
            ("Lot", "ExpirationDate"),
            ("PurchaseInvoiceDueDates", "DueDate"),
            ("PurchaseRate", "CreatedOn"),
            ("PurchaseRate", "UpdatedOn"),
            ("PurchaseRateDetail", "CreatedOn"),
            ("PurchaseRateDetail", "UpdatedOn"),
            ("SalesInvoiceDueDates", "DueDate"),
            ("SalesOrderExternalServiceDetails", "CreatedOn"),
            ("SalesOrderExternalServiceDetails", "UpdatedOn"),
            ("SalesOrderExternalServices", "CreatedOn"),
            ("SalesOrderExternalServices", "UpdatedOn"),
            ("SalesOrderTransports", "CreatedOn"),
            ("SalesOrderTransports", "UpdatedOn"),
            ("UserRefreshTokens", "ExpiryDate"),
            ("WorkOrder", "PlannedDate"),
            ("WorkOrderPhase", "EndTime"),
            ("WorkOrderPhase", "StartTime"),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DropDependentViews(migrationBuilder);
            foreach (var (table, column) in Columns)
            {
                migrationBuilder.Sql(
                    $"""ALTER TABLE "{table}" ALTER COLUMN "{column}" TYPE timestamp without time zone USING "{column}" AT TIME ZONE 'Europe/Madrid';""");
            }
            CreateDependentViews(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropDependentViews(migrationBuilder);
            foreach (var (table, column) in Columns)
            {
                migrationBuilder.Sql(
                    $"""ALTER TABLE "{table}" ALTER COLUMN "{column}" TYPE timestamp with time zone USING "{column}" AT TIME ZONE 'Europe/Madrid';""");
            }
            CreateDependentViews(migrationBuilder);
        }

        private static void DropDependentViews(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP VIEW IF EXISTS public."vw_consolidatedExpenses";""");
            migrationBuilder.Sql("""DROP VIEW IF EXISTS public."vw_consolidatedIncomes";""");
            migrationBuilder.Sql("""DROP VIEW IF EXISTS public."vw_detailedworkorder";""");
        }

        // Current definitions, identical on the staging and pre-production databases.
        private static void CreateDependentViews(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE VIEW public."vw_consolidatedExpenses" AS
                 SELECT a."YearPaymentDate",
                    a."MonthPaymentDate",
                    a."WeekPaymentDate",
                    a."PaymentDate",
                    a."Type",
                    a."TypeDetail",
                    a."Description",
                    a."Amount"
                   FROM ( SELECT date_part('year'::text, exp."PaymentDate") AS "YearPaymentDate",
                            date_part('month'::text, exp."PaymentDate") AS "MonthPaymentDate",
                            date_part('week'::text, exp."PaymentDate") AS "WeekPaymentDate",
                            exp."PaymentDate",
                            'Despesa'::text AS "Type",
                            expt."Name" AS "TypeDetail",
                            exp."Description",
                            exp."Amount"
                           FROM "Expenses" exp
                             JOIN "ExpenseTypes" expt ON exp."ExpenseTypeId" = expt."Id"
                        UNION ALL
                         SELECT date_part('YEAR'::text, pid."DueDate") AS "YearPaymentDate",
                            date_part('MONTH'::text, pid."DueDate") AS "MonthPaymentDate",
                            date_part('WEEK'::text, pid."DueDate") AS "WeekPaymentDate",
                            pid."DueDate" AS "PaymentDate",
                            'Compra'::text AS "Type",
                            sp."TaxName" AS "TypeDetail",
                            concat('Factura número: ', pi."SupplierNumber", ' amb data: ', pi."PurchaseInvoiceDate") AS "Description",
                            pid."Amount"
                           FROM "PurchaseInvoices" pi
                             JOIN "PurchaseInvoiceDueDates" pid ON pi."Id" = pid."PurchaseInvoiceId"
                             JOIN "Suppliers" sp ON pi."SupplierId" = sp."Id") a
                  ORDER BY a."PaymentDate";
                """);

            migrationBuilder.Sql("""
                CREATE VIEW public."vw_consolidatedIncomes" AS
                 SELECT date_part('year'::text, dd."DueDate") AS "Year",
                    date_part('month'::text, dd."DueDate") AS "Month",
                    date_part('week'::text, dd."DueDate") AS "Week",
                    dd."DueDate" AS "Date",
                    'Venta'::text AS "Type",
                    'Factura'::text AS "TypeDetail",
                    (('Factura número: '::text || si."InvoiceNumber"::text) || ' amb data: '::text) || dd."DueDate" AS "Description",
                    dd."Amount"
                   FROM "SalesInvoiceDueDates" dd
                     JOIN "SalesInvoice" si ON dd."SalesInvoiceId" = si."Id";
                """);

            migrationBuilder.Sql("""
                CREATE VIEW public."vw_detailedworkorder" AS
                 SELECT wo."Id" AS "WorkOrderId",
                    wo."Code" AS "WorkOrderCode",
                    st."Name" AS "WorkOrderStatusCode",
                    st."Description" AS "WorkOrderStatusDescription",
                    wo."PlannedQuantity",
                    wo."StartTime" AS "WorkOrderStartTime",
                    wo."EndTime" AS "WorkOrderEndTime",
                    wo."Order" AS "WorkOrderOrder",
                    wo."Comment" AS "WorkOrderComment",
                    wo."PlannedDate",
                    re."Code" AS "ReferenceCode",
                    re."Description" AS "ReferenceDescription",
                    re."Version" AS "ReferenceVersion",
                    re."Cost" AS "ReferenceCost",
                    wp."Id" AS "WorkOrderPhaseId",
                    wp."Code" AS "WorkOrderPhaseCode",
                    wp."Description" AS "WorkOrderPhaseDescription",
                    wp."Comment" AS "WorkOrderPhaseComment",
                    stwp."Name" AS "WorkOrderPhaseStatusCode",
                    stwp."Description" AS "WorkOrderPhaseStatusDescription",
                    wp."StartTime" AS "WorkOrderPhaseStartTime",
                    wp."EndTime" AS "WorkOrderPhaseEndTime",
                    wd."Id" AS "WorkOrderPhaseDetailId",
                    wd."Order" AS "WorkOrderPhaseDetailOrder",
                    wd."EstimatedTime" AS "WorkOrderPhaseDetailEstimatedTime",
                    wd."Comment" AS "WorkOrderPhaseDetailComment",
                    ms."Name" AS "MachineStatusName",
                    ms."Description" AS "MachineStatusDescription",
                    wc."Id" AS "WorkcenterId",
                    wc."Name" AS "WorkcenterName",
                    wc."Description" AS "WorkcenterDescription",
                    wc."costHour" AS "WorkcenterCost",
                        CASE
                            WHEN wp."PreferredWorkcenterId" = wc."Id" THEN true
                            ELSE false
                        END AS "PreferredWorkcenter"
                   FROM "WorkOrder" wo
                     JOIN "Statuses" st ON wo."StatusId" = st."Id"
                     JOIN "References" re ON wo."ReferenceId" = re."Id"
                     JOIN "WorkOrderPhase" wp ON wo."Id" = wp."WorkOrderId"
                     JOIN "Statuses" stwp ON wp."StatusId" = stwp."Id"
                     JOIN "WorkOrderPhaseDetail" wd ON wp."Id" = wd."WorkOrderPhaseId"
                     JOIN "Workcenters" wc ON wc."WorkcenterTypeId" = wp."WorkcenterTypeId"
                     JOIN "MachineStatuses" ms ON wd."MachineStatusId" = ms."Id";
                """);
        }
    }
}
