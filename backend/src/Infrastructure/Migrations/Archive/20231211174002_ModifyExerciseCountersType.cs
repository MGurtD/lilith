using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ModifyExerciseCountersType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SalesOrderCounter",
                table: "Exercises",
                type: "varchar",
                maxLength: 10,
                nullable: false,
                defaultValue: "0",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "SalesInvoiceCounter",
                table: "Exercises",
                type: "varchar",
                maxLength: 10,
                nullable: false,
                defaultValue: "0",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiptCounter",
                table: "Exercises",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "PurchaseInvoiceCounter",
                table: "Exercises",
                type: "varchar",
                maxLength: 10,
                nullable: false,
                defaultValue: "0",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryNoteCounter",
                table: "Exercises",
                type: "varchar",
                maxLength: 10,
                nullable: false,
                defaultValue: "0",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SalesOrderCounter",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 10,
                oldDefaultValue: "0");

            migrationBuilder.AlterColumn<int>(
                name: "SalesInvoiceCounter",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 10,
                oldDefaultValue: "0");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiptCounter",
                table: "Exercises",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseInvoiceCounter",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 10,
                oldDefaultValue: "0");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryNoteCounter",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 10,
                oldDefaultValue: "0");
        }
    }
}
