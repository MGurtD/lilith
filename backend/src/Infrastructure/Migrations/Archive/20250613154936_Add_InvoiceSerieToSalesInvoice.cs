using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_InvoiceSerieToSalesInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceSerieId",
                table: "SalesInvoice",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Length",
                table: "PurchaseInvoiceSeries",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "NextNumber",
                table: "PurchaseInvoiceSeries",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "PurchaseInvoiceSeries",
                type: "varchar",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Suffix",
                table: "PurchaseInvoiceSeries",
                type: "varchar",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoice_InvoiceSerieId",
                table: "SalesInvoice",
                column: "InvoiceSerieId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoice_PurchaseInvoiceSeries_InvoiceSerieId",
                table: "SalesInvoice",
                column: "InvoiceSerieId",
                principalTable: "PurchaseInvoiceSeries",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoice_PurchaseInvoiceSeries_InvoiceSerieId",
                table: "SalesInvoice");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoice_InvoiceSerieId",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "InvoiceSerieId",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "PurchaseInvoiceSeries");

            migrationBuilder.DropColumn(
                name: "NextNumber",
                table: "PurchaseInvoiceSeries");

            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "PurchaseInvoiceSeries");

            migrationBuilder.DropColumn(
                name: "Suffix",
                table: "PurchaseInvoiceSeries");
        }
    }
}
