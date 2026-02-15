using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_WorkOrderAndWorkOrderPhaseToProductionParts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkOrderId",
                table: "ProductionParts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "WorkOrderPhaseId",
                table: "ProductionParts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ProductionParts_WorkOrderId",
                table: "ProductionParts",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionParts_WorkOrderPhaseId",
                table: "ProductionParts",
                column: "WorkOrderPhaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionParts_WorkOrder_WorkOrderId",
                table: "ProductionParts",
                column: "WorkOrderId",
                principalTable: "WorkOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionParts_WorkOrderPhase_WorkOrderPhaseId",
                table: "ProductionParts",
                column: "WorkOrderPhaseId",
                principalTable: "WorkOrderPhase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionParts_WorkOrder_WorkOrderId",
                table: "ProductionParts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionParts_WorkOrderPhase_WorkOrderPhaseId",
                table: "ProductionParts");

            migrationBuilder.DropIndex(
                name: "IX_ProductionParts_WorkOrderId",
                table: "ProductionParts");

            migrationBuilder.DropIndex(
                name: "IX_ProductionParts_WorkOrderPhaseId",
                table: "ProductionParts");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                table: "ProductionParts");

            migrationBuilder.DropColumn(
                name: "WorkOrderPhaseId",
                table: "ProductionParts");
        }
    }
}
