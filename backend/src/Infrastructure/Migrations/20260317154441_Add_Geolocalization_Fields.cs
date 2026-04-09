using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Geolocalization_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DistanceFromSite",
                table: "Suppliers",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Suppliers",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Suppliers",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Sites",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Sites",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "CustomerAddress",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "CustomerAddress",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceFromSite",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "CustomerAddress");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "CustomerAddress");
        }
    }
}
