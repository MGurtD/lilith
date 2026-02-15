using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_CostsOnWorkcenterShiftDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ConcurrentOperatorWorkcenters",
                schema: "data",
                table: "WorkcenterShiftDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ConcurrentWorkorderPhases",
                schema: "data",
                table: "WorkcenterShiftDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "OperatorCost",
                schema: "data",
                table: "WorkcenterShiftDetails",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WorkcenterCost",
                schema: "data",
                table: "WorkcenterShiftDetails",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrentWorkorderPhases",
                schema: "data",
                table: "WorkcenterShiftDetails");

            migrationBuilder.DropColumn(
                name: "OperatorCost",
                schema: "data",
                table: "WorkcenterShiftDetails");

            migrationBuilder.DropColumn(
                name: "WorkcenterCost",
                schema: "data",
                table: "WorkcenterShiftDetails");

            migrationBuilder.AlterColumn<int>(
                name: "ConcurrentOperatorWorkcenters",
                schema: "data",
                table: "WorkcenterShiftDetails",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);
        }
    }
}
