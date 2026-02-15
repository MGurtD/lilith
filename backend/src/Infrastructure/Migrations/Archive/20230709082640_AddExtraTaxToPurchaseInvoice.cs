using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddExtraTaxToPurchaseInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExtraTaxAmount",
                table: "PurchaseInvoices",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ExtraTaxPercentatge",
                table: "PurchaseInvoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraTaxAmount",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "ExtraTaxPercentatge",
                table: "PurchaseInvoices");
        }
    }
}
