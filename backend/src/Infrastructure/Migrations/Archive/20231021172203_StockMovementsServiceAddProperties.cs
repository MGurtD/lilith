using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class StockMovementsServiceAddProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_References_Taxes_TaxId",
                table: "References");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StockMovements",
                type: "varchar",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Diameter",
                table: "StockMovements",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "StockMovements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceId",
                table: "StockMovements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Thickness",
                table: "StockMovements",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<Guid>(
                name: "TaxId",
                table: "References",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferenceTypeId",
                table: "References",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_LocationId",
                table: "StockMovements",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ReferenceId",
                table: "StockMovements",
                column: "ReferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_References_ReferenceTypes_ReferenceTypeId",
                table: "References",
                column: "ReferenceTypeId",
                principalTable: "ReferenceTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_References_Taxes_TaxId",
                table: "References",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Locations_LocationId",
                table: "StockMovements",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_References_ReferenceId",
                table: "StockMovements",
                column: "ReferenceId",
                principalTable: "References",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_References_ReferenceTypes_ReferenceTypeId",
                table: "References");

            migrationBuilder.DropForeignKey(
                name: "FK_References_Taxes_TaxId",
                table: "References");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Locations_LocationId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_References_ReferenceId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_LocationId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_ReferenceId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Diameter",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Thickness",
                table: "StockMovements");

            migrationBuilder.AlterColumn<Guid>(
                name: "TaxId",
                table: "References",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferenceTypeId",
                table: "References",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_References_ReferenceTypes_ReferenceTypeId",
                table: "References",
                column: "ReferenceTypeId",
                principalTable: "ReferenceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_References_Taxes_TaxId",
                table: "References",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
