using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_CustomerToReferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "References",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_References_CustomerId",
                table: "References",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_References_Customers_CustomerId",
                table: "References",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_References_Customers_CustomerId",
                table: "References");

            migrationBuilder.DropIndex(
                name: "IX_References_CustomerId",
                table: "References");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "References");
        }
    }
}
