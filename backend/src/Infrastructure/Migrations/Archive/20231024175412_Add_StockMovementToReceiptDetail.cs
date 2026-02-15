using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_StockMovementToReceiptDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StockMovementId",
                table: "ReceiptDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptDetails_StockMovementId",
                table: "ReceiptDetails",
                column: "StockMovementId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptDetails_StockMovements_StockMovementId",
                table: "ReceiptDetails",
                column: "StockMovementId",
                principalTable: "StockMovements",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptDetails_StockMovements_StockMovementId",
                table: "ReceiptDetails");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptDetails_StockMovementId",
                table: "ReceiptDetails");

            migrationBuilder.DropColumn(
                name: "StockMovementId",
                table: "ReceiptDetails");
        }
    }
}
