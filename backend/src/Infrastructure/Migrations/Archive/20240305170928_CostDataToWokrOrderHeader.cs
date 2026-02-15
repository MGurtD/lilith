using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class CostDataToWokrOrderHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostMachine",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "CostOperator",
                table: "WorkOrder");

            migrationBuilder.AddColumn<decimal>(
                name: "MachineCost",
                table: "WorkOrder",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MachineTime",
                table: "WorkOrder",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCost",
                table: "WorkOrder",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OperatorCost",
                table: "WorkOrder",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OperatorTime",
                table: "WorkOrder",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalQuantity",
                table: "WorkOrder",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MachineCost",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "MachineTime",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "MaterialCost",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "OperatorCost",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "OperatorTime",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "WorkOrder");

            migrationBuilder.AddColumn<int>(
                name: "CostMachine",
                table: "WorkOrder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CostOperator",
                table: "WorkOrder",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
