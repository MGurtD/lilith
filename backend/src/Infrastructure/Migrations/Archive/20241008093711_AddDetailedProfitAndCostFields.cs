using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddDetailedProfitAndCostFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExternalProfit",
                table: "SalesOrderDetail",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCost",
                table: "SalesOrderDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialProfit",
                table: "SalesOrderDetail",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductionCost",
                table: "SalesOrderDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductionProfit",
                table: "SalesOrderDetail",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExternalProfit",
                table: "BudgetDetails",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCost",
                table: "BudgetDetails",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialProfit",
                table: "BudgetDetails",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductionCost",
                table: "BudgetDetails",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductionProfit",
                table: "BudgetDetails",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalProfit",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "MaterialCost",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "MaterialProfit",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "ProductionCost",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "ProductionProfit",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "ExternalProfit",
                table: "BudgetDetails");

            migrationBuilder.DropColumn(
                name: "MaterialCost",
                table: "BudgetDetails");

            migrationBuilder.DropColumn(
                name: "MaterialProfit",
                table: "BudgetDetails");

            migrationBuilder.DropColumn(
                name: "ProductionCost",
                table: "BudgetDetails");

            migrationBuilder.DropColumn(
                name: "ProductionProfit",
                table: "BudgetDetails");
        }
    }
}
