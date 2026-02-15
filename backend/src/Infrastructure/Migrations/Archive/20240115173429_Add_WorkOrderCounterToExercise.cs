using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_WorkOrderCounterToExercise : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "WorkOrder",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "WorkOrder",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "ExerciseId",
                table: "WorkOrder",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedDate",
                table: "WorkOrder",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "WorkOrderCounter",
                table: "Exercises",
                type: "varchar",
                maxLength: 10,
                nullable: false,
                defaultValue: "0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_ExerciseId",
                table: "WorkOrder",
                column: "ExerciseId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_Exercises_ExerciseId",
                table: "WorkOrder",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_Exercises_ExerciseId",
                table: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_ExerciseId",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "PlannedDate",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "WorkOrderCounter",
                table: "Exercises");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "WorkOrder",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "WorkOrder",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
