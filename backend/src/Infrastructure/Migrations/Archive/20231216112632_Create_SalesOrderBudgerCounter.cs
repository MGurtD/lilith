using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Create_SalesOrderBudgerCounter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BudgetNumber",
                table: "SalesOrderHeader",
                type: "varchar",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BudgetCounter",
                table: "Exercises",
                type: "varchar",
                maxLength: 10,
                nullable: false,
                defaultValue: "0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BudgetNumber",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "BudgetCounter",
                table: "Exercises");
        }
    }
}
