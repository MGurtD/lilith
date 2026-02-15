using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_ParentSalesInvoiceRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentSalesInvoiceId",
                table: "SalesInvoice",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoice_ParentSalesInvoiceId",
                table: "SalesInvoice",
                column: "ParentSalesInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoice_SalesInvoice_ParentSalesInvoiceId",
                table: "SalesInvoice",
                column: "ParentSalesInvoiceId",
                principalTable: "SalesInvoice",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoice_SalesInvoice_ParentSalesInvoiceId",
                table: "SalesInvoice");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoice_ParentSalesInvoiceId",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "ParentSalesInvoiceId",
                table: "SalesInvoice");
        }
    }
}
