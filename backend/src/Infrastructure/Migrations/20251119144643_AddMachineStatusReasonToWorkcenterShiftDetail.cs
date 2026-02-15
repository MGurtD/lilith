using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineStatusReasonToWorkcenterShiftDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MachineStatusReasonId",
                schema: "data",
                table: "WorkcenterShiftDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkcenterShiftDetails_MachineStatusReasonId",
                schema: "data",
                table: "WorkcenterShiftDetails",
                column: "MachineStatusReasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkcenterShiftDetails_MachineStatusReasons_MachineStatusRe~",
                schema: "data",
                table: "WorkcenterShiftDetails",
                column: "MachineStatusReasonId",
                principalTable: "MachineStatusReasons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkcenterShiftDetails_MachineStatusReasons_MachineStatusRe~",
                schema: "data",
                table: "WorkcenterShiftDetails");

            migrationBuilder.DropIndex(
                name: "IX_WorkcenterShiftDetails_MachineStatusReasonId",
                schema: "data",
                table: "WorkcenterShiftDetails");

            migrationBuilder.DropColumn(
                name: "MachineStatusReasonId",
                schema: "data",
                table: "WorkcenterShiftDetails");
        }
    }
}
