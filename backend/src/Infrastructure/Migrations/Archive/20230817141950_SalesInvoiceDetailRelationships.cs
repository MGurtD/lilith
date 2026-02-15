using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class SalesInvoiceDetailRelationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceDetails_References_ReferenceId",
                table: "SalesInvoiceDetails");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "SalesInvoiceDetails",
                newName: "TaxId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesInvoiceDetails_ReferenceId",
                table: "SalesInvoiceDetails",
                newName: "IX_SalesInvoiceDetails_TaxId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SalesInvoiceDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderDetailId",
                table: "SalesInvoiceDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDetails_SalesOrderDetailId",
                table: "SalesInvoiceDetails",
                column: "SalesOrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceDetails_SalesOrderDetail_SalesOrderDetailId",
                table: "SalesInvoiceDetails",
                column: "SalesOrderDetailId",
                principalTable: "SalesOrderDetail",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceDetails_Taxes_TaxId",
                table: "SalesInvoiceDetails",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceDetails_SalesOrderDetail_SalesOrderDetailId",
                table: "SalesInvoiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceDetails_Taxes_TaxId",
                table: "SalesInvoiceDetails");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceDetails_SalesOrderDetailId",
                table: "SalesInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SalesInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "SalesOrderDetailId",
                table: "SalesInvoiceDetails");

            migrationBuilder.RenameColumn(
                name: "TaxId",
                table: "SalesInvoiceDetails",
                newName: "ReferenceId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesInvoiceDetails_TaxId",
                table: "SalesInvoiceDetails",
                newName: "IX_SalesInvoiceDetails_ReferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceDetails_References_ReferenceId",
                table: "SalesInvoiceDetails",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
