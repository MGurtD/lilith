using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class RenameOperatorCostPerOperatorHourCost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OperatorCost",
                table: "ProductionParts",
                newName: "OperatorHourCost");

            migrationBuilder.RenameColumn(
                name: "MachineCost",
                table: "ProductionParts",
                newName: "MachineHourCost");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OperatorHourCost",
                table: "ProductionParts",
                newName: "OperatorCost");

            migrationBuilder.RenameColumn(
                name: "MachineHourCost",
                table: "ProductionParts",
                newName: "MachineCost");
        }
    }
}
