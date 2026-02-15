using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Refactor_PurchaseInvoiceStatusToLifecycle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PurchaseInvoiceStatuses_PurchaseInvoiceSta~",
                table: "PurchaseInvoices");

            migrationBuilder.AlterColumn<Guid>(
                name: "PurchaseInvoiceStatusId",
                table: "PurchaseInvoices",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "PurchaseInvoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_StatusId",
                table: "PurchaseInvoices",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PurchaseInvoiceStatuses_PurchaseInvoiceSta~",
                table: "PurchaseInvoices",
                column: "PurchaseInvoiceStatusId",
                principalTable: "PurchaseInvoiceStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Statuses_StatusId",
                table: "PurchaseInvoices",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id");

            migrationBuilder.Sql("update \"PurchaseInvoices\"\r\nset \"StatusId\" = (\r\n\tselect s.\"Id\"\r\n\tfrom \"PurchaseInvoices\" pi\r\n\tjoin \"PurchaseInvoiceStatuses\" pis on pi.\"PurchaseInvoiceStatusId\" = pis.\"Id\"\r\n\tjoin \"Statuses\" s on pis.\"Name\" = s.\"Name\"\r\n\tjoin \"Lifecycles\" l on s.\"LifecycleId\" = l.\"Id\" and l.\"Name\"  = 'PurchaseInvoice'\r\n\twhere \"PurchaseInvoices\".\"Id\" = pi.\"Id\"\r\n)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PurchaseInvoiceStatuses_PurchaseInvoiceSta~",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Statuses_StatusId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_StatusId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "PurchaseInvoices");

            migrationBuilder.AlterColumn<Guid>(
                name: "PurchaseInvoiceStatusId",
                table: "PurchaseInvoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PurchaseInvoiceStatuses_PurchaseInvoiceSta~",
                table: "PurchaseInvoices",
                column: "PurchaseInvoiceStatusId",
                principalTable: "PurchaseInvoiceStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
