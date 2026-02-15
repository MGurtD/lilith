using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class SemanticChangesPurchaseInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PurchaseDate",
                table: "PurchaseInvoices",
                newName: "PurchaseInvoiceDate");

            migrationBuilder.RenameColumn(
                name: "DiscountPercentatge",
                table: "PurchaseInvoices",
                newName: "DiscountPercentage");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PurchaseInvoiceDate",
                table: "PurchaseInvoices",
                newName: "PurchaseDate");

            migrationBuilder.RenameColumn(
                name: "DiscountPercentage",
                table: "PurchaseInvoices",
                newName: "DiscountPercentatge");
        }
    }
}
