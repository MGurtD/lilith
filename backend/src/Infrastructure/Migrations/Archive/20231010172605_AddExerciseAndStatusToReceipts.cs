using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddExerciseAndStatusToReceipts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExerciseId",
                table: "Receipts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "Receipts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ExerciseId",
                table: "Receipts",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_StatusId",
                table: "Receipts",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Exercises_ExerciseId",
                table: "Receipts",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Statuses_StatusId",
                table: "Receipts",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Exercises_ExerciseId",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Statuses_StatusId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_ExerciseId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_StatusId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Receipts");
        }
    }
}
