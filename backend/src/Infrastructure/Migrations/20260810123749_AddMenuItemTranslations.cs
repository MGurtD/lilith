using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuItemTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuItemTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemTranslations_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UK_MenuItemTranslation_MenuItem_Language",
                table: "MenuItemTranslations",
                columns: new[] { "MenuItemId", "LanguageCode" },
                unique: true);

            migrationBuilder.Sql("""
                WITH languages("Code") AS (VALUES ('ca'), ('es'), ('en'))
                INSERT INTO "MenuItemTranslations"
                    ("Id", "MenuItemId", "LanguageCode", "Title", "CreatedOn", "UpdatedOn", "Disabled")
                SELECT
                    md5(menu."Id"::text || ':' || language."Code")::uuid,
                    menu."Id",
                    language."Code",
                    menu."Title",
                    NOW(),
                    NOW(),
                    false
                FROM "MenuItems" menu
                CROSS JOIN languages language;
                """);

            migrationBuilder.DropColumn(
                name: "Title",
                table: "MenuItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "MenuItems",
                type: "varchar",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "MenuItems" menu
                SET "Title" = translation."Title"
                FROM "MenuItemTranslations" translation
                WHERE translation."MenuItemId" = menu."Id"
                  AND translation."LanguageCode" = 'ca';
                """);

            migrationBuilder.DropTable(
                name: "MenuItemTranslations");
        }
    }
}
