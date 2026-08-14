using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailPhaseProfits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetDetailPhaseProfits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkMasterPhaseDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfitPercentage = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetDetailPhaseProfits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetDetailPhaseProfits_BudgetDetails_BudgetDetailId",
                        column: x => x.BudgetDetailId,
                        principalTable: "BudgetDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetailPhaseProfits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkMasterPhaseDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfitPercentage = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDetailPhaseProfits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetailPhaseProfits_SalesOrderDetail_SalesOrderDet~",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetDetailPhaseProfits_BudgetDetailId",
                table: "BudgetDetailPhaseProfits",
                column: "BudgetDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetailPhaseProfits_SalesOrderDetailId",
                table: "SalesOrderDetailPhaseProfits",
                column: "SalesOrderDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetDetailPhaseProfits");

            migrationBuilder.DropTable(
                name: "SalesOrderDetailPhaseProfits");
        }
    }
}
