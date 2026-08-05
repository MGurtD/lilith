using System;
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
            migrationBuilder.Sql(
                """
                DO $migration$
                BEGIN
                    IF (SELECT COUNT(*) FROM "Enterprises" WHERE "Disabled" = false) > 1 THEN
                        RAISE EXCEPTION 'AddEnterpriseBranding requires at most one enabled Enterprise. Resolve the existing data before applying this migration.';
                    END IF;
                END
                $migration$;
                """);

            migrationBuilder.AddColumn<string>(
                name: "BrandName",
                table: "Enterprises",
                type: "varchar",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LogoMainFileId",
                table: "Enterprises",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LogoSidebarFileId",
                table: "Enterprises",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "Enterprises",
                type: "varchar",
                maxLength: 7,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enterprises_LogoMainFileId",
                table: "Enterprises",
                column: "LogoMainFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Enterprises_LogoSidebarFileId",
                table: "Enterprises",
                column: "LogoSidebarFileId");

            migrationBuilder.CreateIndex(
                name: "UK_Enterprise_SingleEnabled",
                table: "Enterprises",
                column: "Disabled",
                unique: true,
                filter: "\"Disabled\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_Enterprises_Files_LogoMainFileId",
                table: "Enterprises",
                column: "LogoMainFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Enterprises_Files_LogoSidebarFileId",
                table: "Enterprises",
                column: "LogoSidebarFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enterprises_Files_LogoMainFileId",
                table: "Enterprises");

            migrationBuilder.DropForeignKey(
                name: "FK_Enterprises_Files_LogoSidebarFileId",
                table: "Enterprises");

            migrationBuilder.DropIndex(
                name: "IX_Enterprises_LogoMainFileId",
                table: "Enterprises");

            migrationBuilder.DropIndex(
                name: "IX_Enterprises_LogoSidebarFileId",
                table: "Enterprises");

            migrationBuilder.DropIndex(
                name: "UK_Enterprise_SingleEnabled",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "BrandName",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "LogoMainFileId",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "LogoSidebarFileId",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "Enterprises");
        }
    }
}
