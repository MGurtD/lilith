using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Refactor_WarehouseDefaultLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Default",
                table: "Locations");

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultLocationId",
                table: "Warehouses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_DefaultLocationId",
                table: "Warehouses",
                column: "DefaultLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Locations_DefaultLocationId",
                table: "Warehouses",
                column: "DefaultLocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Locations_DefaultLocationId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_DefaultLocationId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "DefaultLocationId",
                table: "Warehouses");

            migrationBuilder.AddColumn<bool>(
                name: "Default",
                table: "Locations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
