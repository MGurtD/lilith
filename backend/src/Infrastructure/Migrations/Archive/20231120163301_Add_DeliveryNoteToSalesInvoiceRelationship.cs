using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_DeliveryNoteToSalesInvoiceRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.\"vw_invoiceableOrderDetails\";");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceDetails_SalesOrderDetail_SalesOrderDetailId",
                table: "SalesInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "IsInvoiced",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDate",
                table: "SalesInvoiceDetails");

            migrationBuilder.RenameColumn(
                name: "SalesOrderDetailId",
                table: "SalesInvoiceDetails",
                newName: "DeliveryNoteDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesInvoiceDetails_SalesOrderDetailId",
                table: "SalesInvoiceDetails",
                newName: "IX_SalesInvoiceDetails_DeliveryNoteDetailId");

            migrationBuilder.AddColumn<Guid>(
                name: "SalesInvoiceId",
                table: "DeliveryNotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInvoiced",
                table: "DeliveryNoteDetails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNotes_SalesInvoiceId",
                table: "DeliveryNotes",
                column: "SalesInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_SalesInvoice_SalesInvoiceId",
                table: "DeliveryNotes",
                column: "SalesInvoiceId",
                principalTable: "SalesInvoice",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceDetails_DeliveryNoteDetails_DeliveryNoteDetailId",
                table: "SalesInvoiceDetails",
                column: "DeliveryNoteDetailId",
                principalTable: "DeliveryNoteDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_SalesInvoice_SalesInvoiceId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceDetails_DeliveryNoteDetails_DeliveryNoteDetailId",
                table: "SalesInvoiceDetails");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryNotes_SalesInvoiceId",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "SalesInvoiceId",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "IsInvoiced",
                table: "DeliveryNoteDetails");

            migrationBuilder.RenameColumn(
                name: "DeliveryNoteDetailId",
                table: "SalesInvoiceDetails",
                newName: "SalesOrderDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesInvoiceDetails_DeliveryNoteDetailId",
                table: "SalesInvoiceDetails",
                newName: "IX_SalesInvoiceDetails_SalesOrderDetailId");

            migrationBuilder.AddColumn<bool>(
                name: "IsInvoiced",
                table: "SalesOrderDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryDate",
                table: "SalesInvoiceDetails",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceDetails_SalesOrderDetail_SalesOrderDetailId",
                table: "SalesInvoiceDetails",
                column: "SalesOrderDetailId",
                principalTable: "SalesOrderDetail",
                principalColumn: "Id");

            migrationBuilder.Sql($"-- public.\"vw_invoiceableOrderDetails\" source\r\n\r\nCREATE OR REPLACE VIEW public.\"vw_invoiceableOrderDetails\"\r\nAS SELECT h.\"Id\" AS \"SalesOrderId\",\r\n    h.\"SalesOrderNumber\",\r\n    h.\"SalesOrderDate\",\r\n    h.\"CustomerId\",\r\n    s.\"Id\" AS \"StatusId\",\r\n    s.\"Name\" AS \"StatusName\",\r\n    d.\"Id\",\r\n    d.\"ReferenceId\",\r\n    r.\"Code\" AS \"ReferenceCode\",\r\n    r.\"Description\" AS \"ReferenceDescription\",\r\n    r.\"Version\" AS \"ReferenceVersion\",\r\n    d.\"Quantity\",\r\n    d.\"Description\",\r\n    d.\"UnitCost\",\r\n    d.\"UnitPrice\",\r\n    d.\"TotalCost\",\r\n    d.\"Amount\",\r\n    d.\"IsInvoiced\",\r\n    d.\"IsDelivered\" AS \"IsServed\"\r\n   FROM \"SalesOrderHeader\" h\r\n     JOIN \"SalesOrderDetail\" d ON h.\"Id\" = d.\"SalesOrderHeaderId\"\r\n     JOIN \"References\" r ON d.\"ReferenceId\" = r.\"Id\"\r\n     JOIN \"Statuses\" s ON h.\"StatusId\" = s.\"Id\"\r\n  WHERE d.\"IsInvoiced\" = false;");
        }
    }
}
