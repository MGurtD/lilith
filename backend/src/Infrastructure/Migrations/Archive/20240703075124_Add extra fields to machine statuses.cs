using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Addextrafieldstomachinestatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Closed",
                table: "MachineStatuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "MachineStatuses",
                type: "varchar",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OperatorsAllowed",
                table: "MachineStatuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Preferred",
                table: "MachineStatuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Stoped",
                table: "MachineStatuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Closed",
                table: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "OperatorsAllowed",
                table: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "Preferred",
                table: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "Stoped",
                table: "MachineStatuses");
        }
    }
}
