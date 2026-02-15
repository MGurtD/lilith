using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Fix_WorkMasterEntitiesErrors_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_Workcenters_PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_Workcenters_PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail",
                column: "PreferredWorkcenterId",
                principalTable: "Workcenters",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseDetail_Workcenters_PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseDetail_Workcenters_PreferredWorkcenterId",
                table: "WorkMasterPhaseDetail",
                column: "PreferredWorkcenterId",
                principalTable: "Workcenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
