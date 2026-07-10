using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Enterprises",
                type: "varchar",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<string>(
                name: "LogoMain",
                table: "Enterprises",
                type: "varchar",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoSidebar",
                table: "Enterprises",
                type: "varchar",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "Enterprises",
                type: "varchar",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "Enterprises",
                type: "varchar",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleSidebar",
                table: "Enterprises",
                type: "varchar",
                maxLength: 60,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoMain",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "LogoSidebar",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "TitleSidebar",
                table: "Enterprises");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Enterprises",
                type: "varchar",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 60);
        }
    }
}