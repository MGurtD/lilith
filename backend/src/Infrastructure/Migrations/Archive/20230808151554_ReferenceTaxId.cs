using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ReferenceTaxId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SalesOrderDetail",
                type: "varchar",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TaxId",
                table: "References",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_References_TaxId",
                table: "References",
                column: "TaxId");

            migrationBuilder.AddForeignKey(
                name: "FK_References_Taxes_TaxId",
                table: "References",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_References_Taxes_TaxId",
                table: "References");

            migrationBuilder.DropIndex(
                name: "IX_References_TaxId",
                table: "References");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "References");
        }
    }
}
