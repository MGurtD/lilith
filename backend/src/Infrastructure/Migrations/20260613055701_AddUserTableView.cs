using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTableView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTableViews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Page = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    IsDefault = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    ViewConfig = table.Column<string>(type: "varchar", maxLength: 8000, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTableView", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTableViews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTableView_UserId_Page_IsDefault",
                table: "UserTableViews",
                columns: new[] { "UserId", "Page", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "UK_UserTableView_UserId_Page_Name",
                table: "UserTableViews",
                columns: new[] { "UserId", "Page", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTableViews");
        }
    }
}
