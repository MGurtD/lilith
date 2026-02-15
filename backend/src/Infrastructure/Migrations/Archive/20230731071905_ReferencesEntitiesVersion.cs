using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ReferencesEntitiesVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UK_Reference_Code",
                table: "References");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "References",
                type: "varchar",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "UK_Reference_Code_Version",
                table: "References",
                columns: new[] { "Code", "Version" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UK_Reference_Code_Version",
                table: "References");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "References");

            migrationBuilder.CreateIndex(
                name: "UK_Reference_Code",
                table: "References",
                column: "Code");
        }
    }
}
