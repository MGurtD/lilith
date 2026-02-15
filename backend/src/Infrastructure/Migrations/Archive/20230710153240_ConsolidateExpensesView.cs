using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ConsolidateExpensesView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW public.""vw_consolidatedExpenses""
                    AS SELECT a.""YearPaymentDate"",
                        a.""MonthPaymentDate"",
                        a.""WeekPaymentDate"",
                        a.""PaymentDate"",
                        a.""Type"",
                        a.""TypeDetail"",
                        a.""Description"",
                        a.""Amount""
                    FROM ( SELECT date_part('year'::text, exp.""PaymentDate"") AS ""YearPaymentDate"",
                                date_part('month'::text, exp.""PaymentDate"") AS ""MonthPaymentDate"",
                                date_part('week'::text, exp.""PaymentDate"") AS ""WeekPaymentDate"",
                                exp.""PaymentDate"",
                                'Despesa'::text AS ""Type"",
                                expt.""Name"" AS ""TypeDetail"",
                                exp.""Description"" AS ""Description"",
                                exp.""Amount"" AS ""Amount""
                            FROM ""Expenses"" exp
                                JOIN ""ExpenseTypes"" expt ON exp.""ExpenseTypeId"" = expt.""Id""
                            UNION ALL
                            SELECT date_part('YEAR'::text, pid.""DueDate"") AS ""YearPaymentDate"",
                                date_part('MONTH'::text, pid.""DueDate"") AS ""MonthPaymentDate"",
                                date_part('WEEK'::text, pid.""DueDate"") AS ""WeekPaymentDate"",
                                pid.""DueDate"" AS ""PaymentDate"",
                                'Compra'::text AS ""Type"",
                                sp.""TaxName"" AS ""TypeDetail"",
                                concat('Factura número: ', pi.""SupplierNumber"", ' amb data: ', pi.""PurchaseInvoiceDate"") AS ""Description"",
                                pid.""Amount""
                            FROM ""PurchaseInvoices"" pi
                                JOIN ""PurchaseInvoiceDueDates"" pid ON pi.""Id"" = pid.""PurchaseInvoiceId""
                                JOIN ""Suppliers"" sp ON pi.""SupplierId"" = sp.""Id"") a
                    ORDER BY a.""PaymentDate"";
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW public.""vw_consolidatedExpenses""");
        }
    }
}
