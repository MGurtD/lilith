using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_DefaultSiteToEnterprise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultSiteId",
                table: "Enterprises",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enterprises_DefaultSiteId",
                table: "Enterprises",
                column: "DefaultSiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enterprises_Sites_DefaultSiteId",
                table: "Enterprises",
                column: "DefaultSiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enterprises_Sites_DefaultSiteId",
                table: "Enterprises");

            migrationBuilder.DropIndex(
                name: "IX_Enterprises_DefaultSiteId",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "DefaultSiteId",
                table: "Enterprises");
        }
    }
}
