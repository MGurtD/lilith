using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Change_WorkMasterPhaseAndWorkOrderPhaseForeignKeysNullability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhase_OperatorTypes_OperatorTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhase_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhase_OperatorTypes_OperatorTypeId",
                table: "WorkOrderPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhase_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkOrderPhase");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkcenterTypeId",
                table: "WorkOrderPhase",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "OperatorTypeId",
                table: "WorkOrderPhase",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkcenterTypeId",
                table: "WorkMasterPhase",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "OperatorTypeId",
                table: "WorkMasterPhase",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhase_OperatorTypes_OperatorTypeId",
                table: "WorkMasterPhase",
                column: "OperatorTypeId",
                principalTable: "OperatorTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhase_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkMasterPhase",
                column: "WorkcenterTypeId",
                principalTable: "WorkcenterTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhase_OperatorTypes_OperatorTypeId",
                table: "WorkOrderPhase",
                column: "OperatorTypeId",
                principalTable: "OperatorTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhase_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkOrderPhase",
                column: "WorkcenterTypeId",
                principalTable: "WorkcenterTypes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhase_OperatorTypes_OperatorTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhase_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhase_OperatorTypes_OperatorTypeId",
                table: "WorkOrderPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhase_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkOrderPhase");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkcenterTypeId",
                table: "WorkOrderPhase",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OperatorTypeId",
                table: "WorkOrderPhase",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkcenterTypeId",
                table: "WorkMasterPhase",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OperatorTypeId",
                table: "WorkMasterPhase",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhase_OperatorTypes_OperatorTypeId",
                table: "WorkMasterPhase",
                column: "OperatorTypeId",
                principalTable: "OperatorTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhase_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkMasterPhase",
                column: "WorkcenterTypeId",
                principalTable: "WorkcenterTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhase_OperatorTypes_OperatorTypeId",
                table: "WorkOrderPhase",
                column: "OperatorTypeId",
                principalTable: "OperatorTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhase_WorkcenterTypes_WorkcenterTypeId",
                table: "WorkOrderPhase",
                column: "WorkcenterTypeId",
                principalTable: "WorkcenterTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
