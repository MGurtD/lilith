using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_ReceiptAndPurchaseInvoiceRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseInvoiceId",
                table: "Receipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_PurchaseInvoiceId",
                table: "Receipts",
                column: "PurchaseInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_PurchaseInvoices_PurchaseInvoiceId",
                table: "Receipts",
                column: "PurchaseInvoiceId",
                principalTable: "PurchaseInvoices",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_PurchaseInvoices_PurchaseInvoiceId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_PurchaseInvoiceId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceId",
                table: "Receipts");
        }
    }
}
