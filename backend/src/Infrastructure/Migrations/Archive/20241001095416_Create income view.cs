using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Createincomeview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.\"vw_consolidatedIncomes\"\r\nAS\r\nSELECT \r\n\tdate_part('year', dd.\"DueDate\") as \"Year\",\r\n\tdate_part('month', dd.\"DueDate\") as \"Month\",\r\n\tdate_part('week', dd.\"DueDate\") as \"Week\",\r\n\tdd.\"DueDate\" as \"Date\",\r\n\t'Venta' as \"Type\",\r\n\t'Factura' as \"TypeDetail\",\r\n\t'Factura número: ' || si.\"InvoiceNumber\" || ' amb data: ' || dd.\"DueDate\" as \"Description\",\r\n\tdd.\"Amount\"\r\n\tFROM public.\"SalesInvoiceDueDates\" dd\r\n\tINNER JOIN public.\"SalesInvoice\" si ON dd.\"SalesInvoiceId\" = si.\"Id\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.vw_consolidatedIncomes");
        }
    }
}
