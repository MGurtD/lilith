using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_DeliveryNoteToSalesOrderHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryNoteId",
                table: "SalesOrderHeader",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_DeliveryNoteId",
                table: "SalesOrderHeader",
                column: "DeliveryNoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_DeliveryNotes_DeliveryNoteId",
                table: "SalesOrderHeader",
                column: "DeliveryNoteId",
                principalTable: "DeliveryNotes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_DeliveryNotes_DeliveryNoteId",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_DeliveryNoteId",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "DeliveryNoteId",
                table: "SalesOrderHeader");
        }
    }
}
