using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ProductionParts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    WorkcenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkorderphasedetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionParts_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionParts_Workcenters_WorkcenterId",
                        column: x => x.WorkcenterId,
                        principalTable: "Workcenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionParts_WorkOrderPhaseDetail_WorkorderphasedetailId",
                        column: x => x.WorkorderphasedetailId,
                        principalTable: "WorkOrderPhaseDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_productionpart_date",
                table: "ProductionParts",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "idx_workcenter_phasedetail_operator",
                table: "ProductionParts",
                columns: new[] { "WorkcenterId", "OperatorId", "WorkorderphasedetailId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionParts_OperatorId",
                table: "ProductionParts",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionParts_WorkorderphasedetailId",
                table: "ProductionParts",
                column: "WorkorderphasedetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionParts");
        }
    }
}
