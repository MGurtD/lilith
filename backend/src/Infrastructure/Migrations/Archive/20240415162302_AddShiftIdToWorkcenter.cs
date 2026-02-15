using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddShiftIdToWorkcenter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO public.\"Shifts\"(\"Id\", \"Name\")Values('00000000-0000-0000-0000-000000000000','Undefined');");
            migrationBuilder.AddColumn<Guid>(
                name: "ShiftId",
                table: "Workcenters",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Workcenters_ShiftId",
                table: "Workcenters",
                column: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workcenters_Shifts_ShiftId",
                table: "Workcenters",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workcenters_Shifts_ShiftId",
                table: "Workcenters");

            migrationBuilder.DropIndex(
                name: "IX_Workcenters_ShiftId",
                table: "Workcenters");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "Workcenters");
        }
    }
}
