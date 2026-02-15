using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_BudgetAndBudgetDetails_Refactor_SalesOrderColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BudgetNumber",
                table: "SalesOrderHeader");

            migrationBuilder.RenameColumn(
                name: "SalesOrderNumber",
                table: "SalesOrderHeader",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "SalesOrderDate",
                table: "SalesOrderHeader",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "CustomerSalesOrderNumber",
                table: "SalesOrderHeader",
                newName: "CustomerNumber");

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetId",
                table: "SalesOrderHeader",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "varchar", maxLength: 50, nullable: false, defaultValue: "0"),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DeliveryDays = table.Column<int>(type: "integer", nullable: false),
                    AcceptanceDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgets_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgets_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BudgetDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 2000, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetDetails_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetDetails_References_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "References",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_BudgetId",
                table: "SalesOrderHeader",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetDetails_BudgetId",
                table: "BudgetDetails",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetDetails_ReferenceId",
                table: "BudgetDetails",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CustomerId",
                table: "Budgets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_Date",
                table: "Budgets",
                column: "Date")
                .Annotation("Npgsql:IndexSortOrder", new[] { SortOrder.Descending });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ExerciseId",
                table: "Budgets",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_StatusId",
                table: "Budgets",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_Budgets_BudgetId",
                table: "SalesOrderHeader",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_Budgets_BudgetId",
                table: "SalesOrderHeader");

            migrationBuilder.DropTable(
                name: "BudgetDetails");

            migrationBuilder.DropTable(
                name: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_BudgetId",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "BudgetId",
                table: "SalesOrderHeader");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "SalesOrderHeader",
                newName: "SalesOrderNumber");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "SalesOrderHeader",
                newName: "SalesOrderDate");

            migrationBuilder.RenameColumn(
                name: "CustomerNumber",
                table: "SalesOrderHeader",
                newName: "CustomerSalesOrderNumber");

            migrationBuilder.AddColumn<string>(
                name: "BudgetNumber",
                table: "SalesOrderHeader",
                type: "varchar",
                maxLength: 50,
                nullable: true);
        }
    }
}
