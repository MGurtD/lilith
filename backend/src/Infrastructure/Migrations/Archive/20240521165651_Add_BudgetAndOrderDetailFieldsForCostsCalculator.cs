using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_BudgetAndOrderDetailFieldsForCostsCalculator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "SalesOrderDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Profit",
                table: "SalesOrderDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkMasterId",
                table: "SalesOrderDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "BudgetDetails",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Profit",
                table: "BudgetDetails",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkMasterId",
                table: "BudgetDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetail_WorkMasterId",
                table: "SalesOrderDetail",
                column: "WorkMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetDetails_WorkMasterId",
                table: "BudgetDetails",
                column: "WorkMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetDetails_WorkMaster_WorkMasterId",
                table: "BudgetDetails",
                column: "WorkMasterId",
                principalTable: "WorkMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetail_WorkMaster_WorkMasterId",
                table: "SalesOrderDetail",
                column: "WorkMasterId",
                principalTable: "WorkMaster",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetDetails_WorkMaster_WorkMasterId",
                table: "BudgetDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetail_WorkMaster_WorkMasterId",
                table: "SalesOrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderDetail_WorkMasterId",
                table: "SalesOrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_BudgetDetails_WorkMasterId",
                table: "BudgetDetails");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "Profit",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "WorkMasterId",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "BudgetDetails");

            migrationBuilder.DropColumn(
                name: "Profit",
                table: "BudgetDetails");

            migrationBuilder.DropColumn(
                name: "WorkMasterId",
                table: "BudgetDetails");
        }
    }
}
