using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class CreateFilesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PaymentMethods_PaymentMethodId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PurchaseInvoiceStatuses_PurchaseInvoiceSta~",
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

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentMethodId",
                table: "PurchaseInvoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Entity = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Size = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Path = table.Column<string>(type: "varchar", maxLength: 4000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UK_Files_Entity",
                table: "Files",
                columns: new[] { "EntityId", "Entity" });

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PaymentMethods_PaymentMethodId",
                table: "PurchaseInvoices",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PurchaseInvoiceStatuses_PurchaseInvoiceSta~",
                table: "PurchaseInvoices",
                column: "PurchaseInvoiceStatusId",
                principalTable: "PurchaseInvoiceStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PaymentMethods_PaymentMethodId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PurchaseInvoiceStatuses_PurchaseInvoiceSta~",
                table: "PurchaseInvoices");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.AlterColumn<Guid>(
                name: "PurchaseInvoiceStatusId",
                table: "PurchaseInvoices",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentMethodId",
                table: "PurchaseInvoices",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PaymentMethods_PaymentMethodId",
                table: "PurchaseInvoices",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PurchaseInvoiceStatuses_PurchaseInvoiceSta~",
                table: "PurchaseInvoices",
                column: "PurchaseInvoiceStatusId",
                principalTable: "PurchaseInvoiceStatuses",
                principalColumn: "Id");
        }
    }
}
