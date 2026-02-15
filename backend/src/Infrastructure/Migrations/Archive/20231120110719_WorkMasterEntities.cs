using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class WorkMasterEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Locations_LocationId",
                table: "StockMovements");

            migrationBuilder.AlterColumn<Guid>(
                name: "LocationId",
                table: "StockMovements",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "WorkMaster",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkMaster_References_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "References",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkMasterPhase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseCode = table.Column<string>(type: "varchar", maxLength: 10, nullable: false),
                    PhaseDescription = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    WorkMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkMasterPhase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkMasterPhase_WorkMaster_WorkMasterId",
                        column: x => x.WorkMasterId,
                        principalTable: "WorkMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkMasterPhaseBillOfMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkMasterPhaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    WasteReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Waste = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkMasterPhaseBillOfMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkMasterPhaseBillOfMaterials_References_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "References",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkMasterPhaseBillOfMaterials_References_WasteReferenceId",
                        column: x => x.WasteReferenceId,
                        principalTable: "References",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkMasterPhaseBillOfMaterials_WorkMasterPhase_WorkMasterPh~",
                        column: x => x.WorkMasterPhaseId,
                        principalTable: "WorkMasterPhase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkMasterPhaseDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkMasterPhaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkcenterTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferredWorkcenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperatorTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineStatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstimatedTime = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IsCycleTime = table.Column<bool>(type: "boolean", nullable: false),
                    IsExternalWork = table.Column<bool>(type: "boolean", nullable: false),
                    ExternalWorkCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkMasterPhaseDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkMasterPhaseDetail_MachineStatuses_MachineStatusId",
                        column: x => x.MachineStatusId,
                        principalTable: "MachineStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkMasterPhaseDetail_OperatorTypes_OperatorTypeId",
                        column: x => x.OperatorTypeId,
                        principalTable: "OperatorTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkMasterPhaseDetail_Workcenters_PreferredWorkcenterId",
                        column: x => x.PreferredWorkcenterId,
                        principalTable: "Workcenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkMasterPhaseDetail_WorkcenterTypes_WorkcenterTypeId",
                        column: x => x.WorkcenterTypeId,
                        principalTable: "WorkcenterTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkMasterPhaseDetail_WorkMasterPhase_WorkMasterPhaseId",
                        column: x => x.WorkMasterPhaseId,
                        principalTable: "WorkMasterPhase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkMaster_ReferenceId",
                table: "WorkMaster",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhase_WorkMasterId",
                table: "WorkMasterPhase",
                column: "WorkMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhaseBillOfMaterials_ReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhaseBillOfMaterials_WasteReferenceId",
                table: "WorkMasterPhaseBillOfMaterials",
                column: "WasteReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhaseBillOfMaterials_WorkMasterPhaseId",
                table: "WorkMasterPhaseBillOfMaterials",
                column: "WorkMasterPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhaseDetail_MachineStatusId",
                table: "WorkMasterPhaseDetail",
                column: "MachineStatusId");

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
                name: "IX_WorkMasterPhaseDetail_WorkMasterPhaseId",
                table: "WorkMasterPhaseDetail",
                column: "WorkMasterPhaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Locations_LocationId",
                table: "StockMovements",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Locations_LocationId",
                table: "StockMovements");

            migrationBuilder.DropTable(
                name: "WorkMasterPhaseBillOfMaterials");

            migrationBuilder.DropTable(
                name: "WorkMasterPhaseDetail");

            migrationBuilder.DropTable(
                name: "WorkMasterPhase");

            migrationBuilder.DropTable(
                name: "WorkMaster");

            migrationBuilder.AlterColumn<Guid>(
                name: "LocationId",
                table: "StockMovements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Locations_LocationId",
                table: "StockMovements",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
