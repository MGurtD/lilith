using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Fix_WorkMasterEntitiesErrors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseBillOfMaterials_References_WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderId",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseBillOfMaterials_References_WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                column: "WasteReferenceId",
                principalTable: "References",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhaseBillOfMaterials_References_WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseOrderId",
                table: "WorkMasterPhaseDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhaseBillOfMaterials_References_WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                column: "WasteReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
