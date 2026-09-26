using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RejectionReasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RejectionReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderPhaseRejections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderPhaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    RejectionReasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkcenterShiftDetailId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderPhaseRejections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderPhaseRejections_RejectionReasons_RejectionReasonId",
                        column: x => x.RejectionReasonId,
                        principalTable: "RejectionReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderPhaseRejections_WorkOrderPhase_WorkOrderPhaseId",
                        column: x => x.WorkOrderPhaseId,
                        principalTable: "WorkOrderPhase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderPhaseRejections_WorkcenterShiftDetails_WorkcenterS~",
                        column: x => x.WorkcenterShiftDetailId,
                        principalSchema: "data",
                        principalTable: "WorkcenterShiftDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "UK_RejectionReason_Code",
                table: "RejectionReasons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhaseRejections_RejectionReasonId",
                table: "WorkOrderPhaseRejections",
                column: "RejectionReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhaseRejections_WorkcenterShiftDetailId",
                table: "WorkOrderPhaseRejections",
                column: "WorkcenterShiftDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhaseRejections_WorkOrderPhaseId",
                table: "WorkOrderPhaseRejections",
                column: "WorkOrderPhaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderPhaseRejections");

            migrationBuilder.DropTable(
                name: "RejectionReasons");
        }
    }
}
