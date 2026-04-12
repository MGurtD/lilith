using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityTrackingToStockMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Entity",
                table: "StockMovements",
                type: "varchar",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_entity_entityid",
                table: "StockMovements",
                columns: new[] { "Entity", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_entity_entityid",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Entity",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "StockMovements");
        }
    }
}
