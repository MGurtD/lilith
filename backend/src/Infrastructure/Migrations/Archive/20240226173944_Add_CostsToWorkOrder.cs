using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_CostsToWorkOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetail_WorkOrder_WorkOrderId",
                table: "SalesOrderDetail");

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

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetail_WorkOrder_WorkOrderId",
                table: "SalesOrderDetail",
                column: "WorkOrderId",
                principalTable: "WorkOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetail_WorkOrder_WorkOrderId",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "CostMachine",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "CostOperator",
                table: "WorkOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetail_WorkOrder_WorkOrderId",
                table: "SalesOrderDetail",
                column: "WorkOrderId",
                principalTable: "WorkOrder",
                principalColumn: "Id");
        }
    }
}
