using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Change_SalesOrderDetailDeliveryNoteDetailForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNoteDetails_SalesOrderDetail_SalesOrderDetailId",
                table: "DeliveryNoteDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "SalesOrderDetailId",
                table: "DeliveryNoteDetails",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNoteDetails_SalesOrderDetail_SalesOrderDetailId",
                table: "DeliveryNoteDetails",
                column: "SalesOrderDetailId",
                principalTable: "SalesOrderDetail",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNoteDetails_SalesOrderDetail_SalesOrderDetailId",
                table: "DeliveryNoteDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "SalesOrderDetailId",
                table: "DeliveryNoteDetails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNoteDetails_SalesOrderDetail_SalesOrderDetailId",
                table: "DeliveryNoteDetails",
                column: "SalesOrderDetailId",
                principalTable: "SalesOrderDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
