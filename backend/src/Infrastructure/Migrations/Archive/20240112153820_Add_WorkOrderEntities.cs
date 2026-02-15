using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_WorkOrderEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalWorkCost",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropColumn(
                name: "IsExternalWork",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.RenameColumn(
                name: "PhaseDescription",
                table: "WorkMasterPhase",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "PhaseCode",
                table: "WorkMasterPhase",
                newName: "Code");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "WorkMasterPhase",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExternalWorkCost",
                table: "WorkMasterPhase",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalWork",
                table: "WorkMasterPhase",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WorkOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderHeaderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrderDetailId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Comment = table.Column<string>(type: "varchar", maxLength: 4000, nullable: false, defaultValue: ""),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrder_References_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "References",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrder_SalesOrderDetail_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrder_SalesOrderHeader_SalesOrderHeaderId",
                        column: x => x.SalesOrderHeaderId,
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrder_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrder_WorkMaster_WorkMasterId",
                        column: x => x.WorkMasterId,
                        principalTable: "WorkMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderPhase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "varchar", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperatorTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkcenterTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferredWorkcenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsExternalWork = table.Column<bool>(type: "boolean", nullable: false),
                    ExternalWorkCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderPhase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderPhase_OperatorTypes_OperatorTypeId",
                        column: x => x.OperatorTypeId,
                        principalTable: "OperatorTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderPhase_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderPhase_Workcenters_PreferredWorkcenterId",
                        column: x => x.PreferredWorkcenterId,
                        principalTable: "Workcenters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderPhase_WorkcenterTypes_WorkcenterTypeId",
                        column: x => x.WorkcenterTypeId,
                        principalTable: "WorkcenterTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderPhase_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderPhaseBillOfMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderPhaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Width = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Length = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Height = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Diameter = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Thickness = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderPhaseBillOfMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderPhaseBillOfMaterials_References_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "References",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderPhaseBillOfMaterials_WorkOrderPhase_WorkOrderPhase~",
                        column: x => x.WorkOrderPhaseId,
                        principalTable: "WorkOrderPhase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderPhaseDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderPhaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineStatusId = table.Column<Guid>(type: "uuid", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsCycleTime = table.Column<bool>(type: "boolean", nullable: false),
                    EstimatedTime = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderPhaseDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderPhaseDetail_MachineStatuses_MachineStatusId",
                        column: x => x.MachineStatusId,
                        principalTable: "MachineStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderPhaseDetail_WorkOrderPhase_WorkOrderPhaseId",
                        column: x => x.WorkOrderPhaseId,
                        principalTable: "WorkOrderPhase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_ReferenceId",
                table: "WorkOrder",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_SalesOrderDetailId",
                table: "WorkOrder",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_SalesOrderHeaderId",
                table: "WorkOrder",
                column: "SalesOrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_StatusId",
                table: "WorkOrder",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_WorkMasterId",
                table: "WorkOrder",
                column: "WorkMasterId");

            migrationBuilder.CreateIndex(
                name: "UK_WorkOrder",
                table: "WorkOrder",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhase_OperatorTypeId",
                table: "WorkOrderPhase",
                column: "OperatorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhase_PreferredWorkcenterId",
                table: "WorkOrderPhase",
                column: "PreferredWorkcenterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhase_StatusId",
                table: "WorkOrderPhase",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhase_WorkcenterTypeId",
                table: "WorkOrderPhase",
                column: "WorkcenterTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhase_WorkOrderId",
                table: "WorkOrderPhase",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhaseBillOfMaterials_ReferenceId",
                table: "WorkOrderPhaseBillOfMaterials",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhaseBillOfMaterials_WorkOrderPhaseId",
                table: "WorkOrderPhaseBillOfMaterials",
                column: "WorkOrderPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhaseDetail_MachineStatusId",
                table: "WorkOrderPhaseDetail",
                column: "MachineStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhaseDetail_WorkOrderPhaseId",
                table: "WorkOrderPhaseDetail",
                column: "WorkOrderPhaseId");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderPhaseBillOfMaterials");

            migrationBuilder.DropTable(
                name: "WorkOrderPhaseDetail");

            migrationBuilder.DropTable(
                name: "WorkOrderPhase");

            migrationBuilder.DropTable(
                name: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "WorkMasterPhaseDetail");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "ExternalWorkCost",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "IsExternalWork",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "OperatorTypeId",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "PreferredWorkcenterId",
                table: "WorkMasterPhase");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "WorkMasterPhase",
                newName: "PhaseDescription");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "WorkMasterPhase",
                newName: "PhaseCode");

            migrationBuilder.AddColumn<decimal>(
                name: "ExternalWorkCost",
                table: "WorkMasterPhaseDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalWork",
                table: "WorkMasterPhaseDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
