using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhaseTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhaseTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhaseTemplateDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineStatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseTemplateDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhaseTemplateDetail_MachineStatuses_MachineStatusId",
                        column: x => x.MachineStatusId,
                        principalTable: "MachineStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhaseTemplateDetail_PhaseTemplate_PhaseTemplateId",
                        column: x => x.PhaseTemplateId,
                        principalTable: "PhaseTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhaseTemplateDetail_MachineStatusId",
                table: "PhaseTemplateDetail",
                column: "MachineStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseTemplateDetail_PhaseTemplateId",
                table: "PhaseTemplateDetail",
                column: "PhaseTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhaseTemplateDetail");

            migrationBuilder.DropTable(
                name: "PhaseTemplate");
        }
    }
}
