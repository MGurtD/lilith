using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class CostToOperatorTypeRemoveOperatorCost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperatorCosts");

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "OperatorTypes",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "OperatorTypes");

            migrationBuilder.CreateTable(
                name: "OperatorCosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineStatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperatorTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorCost", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatorCosts_MachineStatuses_MachineStatusId",
                        column: x => x.MachineStatusId,
                        principalTable: "MachineStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperatorCosts_OperatorTypes_OperatorTypeId",
                        column: x => x.OperatorTypeId,
                        principalTable: "OperatorTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorCosts_MachineStatusId",
                table: "OperatorCosts",
                column: "MachineStatusId");

            migrationBuilder.CreateIndex(
                name: "UK_OperatorCosts_Operator_MachineStatus",
                table: "OperatorCosts",
                columns: new[] { "OperatorTypeId", "MachineStatusId" },
                unique: true);
        }
    }
}
