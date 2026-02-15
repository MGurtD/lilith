using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Addprofitpercentagetoworkcenter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ProfitPercentage",
                table: "Workcenters",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("UPDATE public.\"Workcenters\" wc\r\nSET \"ProfitPercentage\" = wct.\"ProfitPercentage\"\r\nFROM public.\"WorkcenterTypes\" wct\r\nWHERE wc.\"WorkcenterTypeId\" = wct.\"Id\";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfitPercentage",
                table: "Workcenters");
        }
    }
}
