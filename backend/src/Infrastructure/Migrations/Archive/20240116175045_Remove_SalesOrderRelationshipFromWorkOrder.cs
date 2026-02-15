using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Remove_SalesOrderRelationshipFromWorkOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_SalesOrderDetail_SalesOrderDetailId",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_SalesOrderHeader_SalesOrderHeaderId",
                table: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_SalesOrderDetailId",
                table: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_SalesOrderHeaderId",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "SalesOrderDetailId",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "SalesOrderHeaderId",
                table: "WorkOrder");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderDetailId",
                table: "WorkOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderHeaderId",
                table: "WorkOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_SalesOrderDetailId",
                table: "WorkOrder",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_SalesOrderHeaderId",
                table: "WorkOrder",
                column: "SalesOrderHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_SalesOrderDetail_SalesOrderDetailId",
                table: "WorkOrder",
                column: "SalesOrderDetailId",
                principalTable: "SalesOrderDetail",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_SalesOrderHeader_SalesOrderHeaderId",
                table: "WorkOrder",
                column: "SalesOrderHeaderId",
                principalTable: "SalesOrderHeader",
                principalColumn: "Id");
        }
    }
}
