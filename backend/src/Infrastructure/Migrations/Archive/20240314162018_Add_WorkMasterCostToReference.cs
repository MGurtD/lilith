using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_WorkMasterCostToReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastPurchaseCost",
                table: "References",
                newName: "WorkMasterCost");

            migrationBuilder.AddColumn<decimal>(
                name: "LastCost",
                table: "References",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCost",
                table: "References");

            migrationBuilder.RenameColumn(
                name: "WorkMasterCost",
                table: "References",
                newName: "LastPurchaseCost");
        }
    }
}
