using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_SalesOrderDetailWorkOrderRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkOrderId",
                table: "SalesOrderDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetail_WorkOrderId",
                table: "SalesOrderDetail",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetail_WorkOrder_WorkOrderId",
                table: "SalesOrderDetail",
                column: "WorkOrderId",
                principalTable: "WorkOrder",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetail_WorkOrder_WorkOrderId",
                table: "SalesOrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderDetail_WorkOrderId",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                table: "SalesOrderDetail");
        }
    }
}
