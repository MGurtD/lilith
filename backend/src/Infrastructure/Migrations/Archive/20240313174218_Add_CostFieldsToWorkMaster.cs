using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_CostFieldsToWorkMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "externalCost",
                table: "WorkMaster",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "machineCost",
                table: "WorkMaster",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "materialCost",
                table: "WorkMaster",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "operatorCost",
                table: "WorkMaster",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "externalCost",
                table: "WorkMaster");

            migrationBuilder.DropColumn(
                name: "machineCost",
                table: "WorkMaster");

            migrationBuilder.DropColumn(
                name: "materialCost",
                table: "WorkMaster");

            migrationBuilder.DropColumn(
                name: "operatorCost",
                table: "WorkMaster");
        }
    }
}
