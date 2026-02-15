using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_SupplierReferenceEntity_Part2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierReferences_ReferenceId",
                table: "SupplierReferences");

            migrationBuilder.AddColumn<decimal>(
                name: "SupplierPrice",
                table: "SupplierReferences",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SupplyDays",
                table: "SupplierReferences",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReferences_ReferenceId_SupplierId",
                table: "SupplierReferences",
                columns: new[] { "ReferenceId", "SupplierId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierReferences_ReferenceId_SupplierId",
                table: "SupplierReferences");

            migrationBuilder.DropColumn(
                name: "SupplierPrice",
                table: "SupplierReferences");

            migrationBuilder.DropColumn(
                name: "SupplyDays",
                table: "SupplierReferences");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReferences_ReferenceId",
                table: "SupplierReferences",
                column: "ReferenceId");
        }
    }
}
