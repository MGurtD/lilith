using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ChangeDocumentNumberEntitiesAndBuildersToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SalesOrderNumber",
                table: "SalesOrderHeader",
                type: "varchar",
                maxLength: 50,
                nullable: false,
                defaultValue: "0",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNumber",
                table: "SalesInvoice",
                type: "varchar",
                maxLength: 50,
                nullable: false,
                defaultValue: "0",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Receipts",
                type: "varchar",
                maxLength: 50,
                nullable: false,
                defaultValue: "0",
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "PurchaseInvoices",
                type: "varchar",
                maxLength: 50,
                nullable: false,
                defaultValue: "0",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "DeliveryNotes",
                type: "varchar",
                maxLength: 50,
                nullable: false,
                defaultValue: "0",
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 25);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SalesOrderNumber",
                table: "SalesOrderHeader",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 50,
                oldDefaultValue: "0");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceNumber",
                table: "SalesInvoice",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 50,
                oldDefaultValue: "0");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Receipts",
                type: "varchar",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 50,
                oldDefaultValue: "0");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "PurchaseInvoices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 50,
                oldDefaultValue: "0");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "DeliveryNotes",
                type: "varchar",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 50,
                oldDefaultValue: "0");
        }
    }
}
