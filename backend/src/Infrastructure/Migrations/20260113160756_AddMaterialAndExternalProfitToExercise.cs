using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialAndExternalProfitToExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExternalProfit",
                table: "Exercises",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 30m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialProfit",
                table: "Exercises",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 30m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalProfit",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "MaterialProfit",
                table: "Exercises");
        }
    }
}
