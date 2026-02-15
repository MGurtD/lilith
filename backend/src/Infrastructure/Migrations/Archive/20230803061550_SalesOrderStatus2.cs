using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class SalesOrderStatus2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_StatusId",
                table: "SalesOrderHeader",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_Statuses_StatusId",
                table: "SalesOrderHeader",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_Statuses_StatusId",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_StatusId",
                table: "SalesOrderHeader");
        }
    }
}
