using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Change_WorkMasterPhaseDetailMachineStatusForeignKeyNullability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "MachineStatusId",
                table: "WorkMasterPhaseDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail",
                column: "MachineStatusId",
                principalTable: "MachineStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "MachineStatusId",
                table: "WorkMasterPhaseDetail",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                table: "WorkMasterPhaseDetail",
                column: "MachineStatusId",
                principalTable: "MachineStatuses",
                principalColumn: "Id");
        }
    }
}
