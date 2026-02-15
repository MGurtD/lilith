using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class CreateProfitPercentagefieldintoworkcentertypeworkmasterphaseandworkorderphase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ProfitPercentage",
                table: "WorkOrderPhase",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProfitPercentage",
                table: "WorkMasterPhase",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProfitPercentage",
                table: "WorkcenterTypes",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfitPercentage",
                table: "WorkOrderPhase");

            migrationBuilder.DropColumn(
                name: "ProfitPercentage",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "ProfitPercentage",
                table: "WorkcenterTypes");
        }
    }
}
