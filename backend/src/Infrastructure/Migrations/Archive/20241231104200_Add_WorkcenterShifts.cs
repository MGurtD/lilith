using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_WorkcenterShifts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "data");

            migrationBuilder.CreateTable(
                name: "WorkcenterShifts",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkcenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    Current = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkcenterShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkcenterShifts_ShiftDetails_ShiftDetailId",
                        column: x => x.ShiftDetailId,
                        principalTable: "ShiftDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkcenterShifts_Workcenters_WorkcenterId",
                        column: x => x.WorkcenterId,
                        principalTable: "Workcenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkcenterShiftDetails",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkcenterShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineStatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkOrderPhaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityOk = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    QuantityKo = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ConcurrentOperatorWorkcenters = table.Column<int>(type: "integer", nullable: false),
                    Current = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkcenterShiftDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkcenterShiftDetails_MachineStatuses_MachineStatusId",
                        column: x => x.MachineStatusId,
                        principalTable: "MachineStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkcenterShiftDetails_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkcenterShiftDetails_WorkcenterShifts_WorkcenterShiftId",
                        column: x => x.WorkcenterShiftId,
                        principalSchema: "data",
                        principalTable: "WorkcenterShifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkcenterShiftDetails_WorkOrderPhase_WorkOrderPhaseId",
                        column: x => x.WorkOrderPhaseId,
                        principalTable: "WorkOrderPhase",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkcenterShiftDetails_MachineStatusId",
                schema: "data",
                table: "WorkcenterShiftDetails",
                column: "MachineStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkcenterShiftDetails_OperatorId",
                schema: "data",
                table: "WorkcenterShiftDetails",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkcenterShiftDetails_Unique",
                schema: "data",
                table: "WorkcenterShiftDetails",
                columns: new[] { "WorkcenterShiftId", "MachineStatusId", "OperatorId", "WorkOrderPhaseId", "StartTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkcenterShiftDetails_WorkOrderPhaseId",
                schema: "data",
                table: "WorkcenterShiftDetails",
                column: "WorkOrderPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkcenterShifts_ShiftDetailId",
                schema: "data",
                table: "WorkcenterShifts",
                column: "ShiftDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkcenterShifts_Unique",
                schema: "data",
                table: "WorkcenterShifts",
                columns: new[] { "WorkcenterId", "StartTime" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkcenterShiftDetails",
                schema: "data");

            migrationBuilder.DropTable(
                name: "WorkcenterShifts",
                schema: "data");
        }
    }
}
