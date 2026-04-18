using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateCountryToIsoCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Sites\" SET \"Country\" = 'ES' WHERE \"Country\" IN ('Espanya', 'España')");
            migrationBuilder.Sql(
                "UPDATE \"CustomerAddress\" SET \"Country\" = 'ES' WHERE \"Country\" IN ('Espanya', 'España')");
            migrationBuilder.Sql(
                "UPDATE \"Suppliers\" SET \"Country\" = 'ES' WHERE \"Country\" IN ('Espanya', 'España')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Sites\" SET \"Country\" = 'Espanya' WHERE \"Country\" = 'ES'");
            migrationBuilder.Sql(
                "UPDATE \"CustomerAddress\" SET \"Country\" = 'Espanya' WHERE \"Country\" = 'ES'");
            migrationBuilder.Sql(
                "UPDATE \"Suppliers\" SET \"Country\" = 'Espanya' WHERE \"Country\" = 'ES'");
        }
    }
}
