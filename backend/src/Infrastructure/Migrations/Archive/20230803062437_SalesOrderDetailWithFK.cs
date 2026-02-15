using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class SalesOrderDetailWithFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetail_SalesOrderHeader_SalesOrderHeaderId",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "SalesOrderId",
                table: "SalesOrderDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "SalesOrderHeaderId",
                table: "SalesOrderDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetail_SalesOrderHeader_SalesOrderHeaderId",
                table: "SalesOrderDetail",
                column: "SalesOrderHeaderId",
                principalTable: "SalesOrderHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetail_SalesOrderHeader_SalesOrderHeaderId",
                table: "SalesOrderDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "SalesOrderHeaderId",
                table: "SalesOrderDetail",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderId",
                table: "SalesOrderDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetail_SalesOrderHeader_SalesOrderHeaderId",
                table: "SalesOrderDetail",
                column: "SalesOrderHeaderId",
                principalTable: "SalesOrderHeader",
                principalColumn: "Id");
        }
    }
}
