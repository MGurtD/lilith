using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseReportWatermark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LogoWatermarkFileId",
                table: "Enterprises",
                type: "uuid",
                nullable: true);

            // Existing installations keep printing the watermark they have today.
            migrationBuilder.AddColumn<bool>(
                name: "ReportWatermarkEnabled",
                table: "Enterprises",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enterprises_LogoWatermarkFileId",
                table: "Enterprises",
                column: "LogoWatermarkFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enterprises_Files_LogoWatermarkFileId",
                table: "Enterprises",
                column: "LogoWatermarkFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enterprises_Files_LogoWatermarkFileId",
                table: "Enterprises");

            migrationBuilder.DropIndex(
                name: "IX_Enterprises_LogoWatermarkFileId",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "LogoWatermarkFileId",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "ReportWatermarkEnabled",
                table: "Enterprises");
        }
    }
}
