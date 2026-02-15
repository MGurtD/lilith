using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Refactor_WorkmasterPhaseEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseBillOfMaterials_References_WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_OperatorTypes_OperatorTypeId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_Workcenters_PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropIndex(
                name: "IX_WorkMasterPhaseDetail_OperatorTypeId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropIndex(
                name: "IX_WorkMasterPhaseDetail_PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropIndex(
                name: "IX_WorkMasterPhaseDetail_WorkcenterTypeId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropIndex(
                name: "IX_WorkMasterPhaseBillOfMaterials_WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.DropColumn(
                name: "OperatorTypeId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropColumn(
                name: "PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropColumn(
                name: "WorkcenterTypeId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropColumn(
                name: "WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.RenameColumn(
                name: "Waste",
                table: "WorkMasterPhaseBillOfMaterials",
                newName: "Width");

            migrationBuilder.AlterColumn<Guid>(
                name: "MachineStatusId",
                table: "WorkMasterPhaseDetail",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "WorkMasterPhaseDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "WorkMasterPhaseDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Diameter",
                table: "WorkMasterPhaseBillOfMaterials",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "WorkMasterPhaseBillOfMaterials",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                table: "WorkMasterPhaseBillOfMaterials",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Thickness",
                table: "WorkMasterPhaseBillOfMaterials",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorTypeId",
                table: "WorkMasterPhase",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredWorkcenterId",
                table: "WorkMasterPhase",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkcenterTypeId",
                table: "WorkMasterPhase",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhase_OperatorTypeId",
                table: "WorkMasterPhase",
                column: "OperatorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhase_PreferredWorkcenterId",
                table: "WorkMasterPhase",
                column: "PreferredWorkcenterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhase_WorkcenterTypeId",
                table: "WorkMasterPhase",
                column: "WorkcenterTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhase_OperatorTypes_OperatorTypeId",
                table: "WorkMasterPhase",
                column: "OperatorTypeId",
                principalTable: "OperatorTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhase_Workcenters_PreferredWorkcenterId",
                table: "WorkMasterPhase",
                column: "PreferredWorkcenterId",
                principalTable: "Workcenters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhase_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkMasterPhase",
                column: "WorkcenterTypeId",
                principalTable: "WorkcenterTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail",
                column: "MachineStatusId",
                principalTable: "MachineStatuses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhase_OperatorTypes_OperatorTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhase_Workcenters_PreferredWorkcenterId",
                table: "WorkMasterPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhase_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropIndex(
                name: "IX_WorkMasterPhase_OperatorTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.DropIndex(
                name: "IX_WorkMasterPhase_PreferredWorkcenterId",
                table: "WorkMasterPhase");

            migrationBuilder.DropIndex(
                name: "IX_WorkMasterPhase_WorkcenterTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropColumn(
                name: "Diameter",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.DropColumn(
                name: "Thickness",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.DropColumn(
                name: "OperatorTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "PreferredWorkcenterId",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "WorkcenterTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.RenameColumn(
                name: "Width",
                table: "WorkMasterPhaseBillOfMaterials",
                newName: "Waste");

            migrationBuilder.AlterColumn<Guid>(
                name: "MachineStatusId",
                table: "WorkMasterPhaseDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorTypeId",
                table: "WorkMasterPhaseDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkcenterTypeId",
                table: "WorkMasterPhaseDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhaseDetail_OperatorTypeId",
                table: "WorkMasterPhaseDetail",
                column: "OperatorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhaseDetail_PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail",
                column: "PreferredWorkcenterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhaseDetail_WorkcenterTypeId",
                table: "WorkMasterPhaseDetail",
                column: "WorkcenterTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhaseBillOfMaterials_WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                column: "WasteReferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseBillOfMaterials_References_WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                column: "WasteReferenceId",
                principalTable: "References",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail",
                column: "MachineStatusId",
                principalTable: "MachineStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_OperatorTypes_OperatorTypeId",
                table: "WorkMasterPhaseDetail",
                column: "OperatorTypeId",
                principalTable: "OperatorTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_Workcenters_PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail",
                column: "PreferredWorkcenterId",
                principalTable: "Workcenters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkMasterPhaseDetail",
                column: "WorkcenterTypeId",
                principalTable: "WorkcenterTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
