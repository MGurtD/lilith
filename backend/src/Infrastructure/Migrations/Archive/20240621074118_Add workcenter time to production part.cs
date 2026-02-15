using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Addworkcentertimetoproductionpart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Time",
                table: "ProductionParts",
                newName: "WorkcenterTime");

            migrationBuilder.AddColumn<decimal>(
                name: "OperatorTime",
                table: "ProductionParts",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatorTime",
                table: "ProductionParts");

            migrationBuilder.RenameColumn(
                name: "WorkcenterTime",
                table: "ProductionParts",
                newName: "Time");
        }
    }
}
