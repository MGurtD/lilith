using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Fix_ProductionPartSintaxis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionParts_WorkOrderPhaseDetail_WorkorderphasedetailId",
                table: "ProductionParts");

            migrationBuilder.RenameColumn(
                name: "WorkorderphasedetailId",
                table: "ProductionParts",
                newName: "WorkOrderPhaseDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionParts_WorkorderphasedetailId",
                table: "ProductionParts",
                newName: "IX_ProductionParts_WorkOrderPhaseDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionParts_WorkOrderPhaseDetail_WorkOrderPhaseDetailId",
                table: "ProductionParts",
                column: "WorkOrderPhaseDetailId",
                principalTable: "WorkOrderPhaseDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionParts_WorkOrderPhaseDetail_WorkOrderPhaseDetailId",
                table: "ProductionParts");

            migrationBuilder.RenameColumn(
                name: "WorkOrderPhaseDetailId",
                table: "ProductionParts",
                newName: "WorkorderphasedetailId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionParts_WorkOrderPhaseDetailId",
                table: "ProductionParts",
                newName: "IX_ProductionParts_WorkorderphasedetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionParts_WorkOrderPhaseDetail_WorkorderphasedetailId",
                table: "ProductionParts",
                column: "WorkorderphasedetailId",
                principalTable: "WorkOrderPhaseDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
