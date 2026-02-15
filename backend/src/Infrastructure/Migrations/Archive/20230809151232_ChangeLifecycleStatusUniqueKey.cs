using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ChangeLifecycleStatusUniqueKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Statuses_LifecycleId",
                table: "Statuses");

            migrationBuilder.DropIndex(
                name: "UK_Statuses_Name",
                table: "Statuses");

            migrationBuilder.CreateIndex(
                name: "UK_Statuses",
                table: "Statuses",
                columns: new[] { "LifecycleId", "Name" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UK_Statuses",
                table: "Statuses");

            migrationBuilder.CreateIndex(
                name: "IX_Statuses_LifecycleId",
                table: "Statuses",
                column: "LifecycleId");

            migrationBuilder.CreateIndex(
                name: "UK_Statuses_Name",
                table: "Statuses",
                column: "Name",
                unique: true);
        }
    }
}
