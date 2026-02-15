using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddAreaIdtoreference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AreaId",
                table: "References",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_References_AreaId",
                table: "References",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_References_Areas_AreaId",
                table: "References",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_References_Areas_AreaId",
                table: "References");

            migrationBuilder.DropIndex(
                name: "IX_References_AreaId",
                table: "References");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "References");
        }
    }
}
