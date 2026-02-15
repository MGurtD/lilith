using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class RemoveTaxFromPurchaseInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Taxes_TaxId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_TaxId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "PurchaseInvoices");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "PurchaseInvoices",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxId",
                table: "PurchaseInvoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_TaxId",
                table: "PurchaseInvoices",
                column: "TaxId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Taxes_TaxId",
                table: "PurchaseInvoices",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id");
        }
    }
}
