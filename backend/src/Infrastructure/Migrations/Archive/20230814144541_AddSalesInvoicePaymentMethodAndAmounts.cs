using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddSalesInvoicePaymentMethodAndAmounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseAmount",
                table: "SalesInvoice",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossAmount",
                table: "SalesInvoice",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "SalesInvoice",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodId",
                table: "SalesInvoice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "TransportAmount",
                table: "SalesInvoice",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoice_PaymentMethodId",
                table: "SalesInvoice",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoice_PaymentMethods_PaymentMethodId",
                table: "SalesInvoice",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoice_PaymentMethods_PaymentMethodId",
                table: "SalesInvoice");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoice_PaymentMethodId",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "BaseAmount",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "GrossAmount",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "TransportAmount",
                table: "SalesInvoice");
        }
    }
}
