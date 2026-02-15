using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_VerifactuRequestWithStatusAndQr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QrCodeBase64",
                table: "SalesInvoiceVerifactuRequest",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeUrl",
                table: "SalesInvoiceVerifactuRequest",
                type: "varchar",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "SalesInvoiceVerifactuRequest",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrCodeBase64",
                table: "SalesInvoiceVerifactuRequest");

            migrationBuilder.DropColumn(
                name: "QrCodeUrl",
                table: "SalesInvoiceVerifactuRequest");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "SalesInvoiceVerifactuRequest");

        }
    }
}
