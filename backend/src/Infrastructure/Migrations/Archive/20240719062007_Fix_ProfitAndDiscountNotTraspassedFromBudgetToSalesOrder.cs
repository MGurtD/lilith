using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Fix_ProfitAndDiscountNotTraspassedFromBudgetToSalesOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"SalesOrderDetail\" OD\r\nSET \r\n    \"Profit\" = BD.\"Profit\",\r\n    \"Discount\" = BD.\"Discount\"\r\nFROM \r\n    \"BudgetDetails\" BD \r\n    JOIN \"Budgets\" B ON BD.\"BudgetId\" = B.\"Id\"\r\n    JOIN \"SalesOrderHeader\" O ON O.\"BudgetId\" = B.\"Id\"\r\nWHERE\r\n    OD.\"SalesOrderHeaderId\" = O.\"Id\" AND\r\n    BD.\"ReferenceId\" = OD.\"ReferenceId\" AND\r\n    (BD.\"Profit\" <> OD.\"Profit\" OR BD.\"Discount\" <> OD.\"Discount\");");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
