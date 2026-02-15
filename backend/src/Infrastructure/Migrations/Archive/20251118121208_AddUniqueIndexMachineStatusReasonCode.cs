using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexMachineStatusReasonCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UK_MachineStatusReason_Code_MachineStatusId",
                table: "MachineStatusReasons",
                columns: new[] { "Code", "MachineStatusId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UK_MachineStatusReason_Code_MachineStatusId",
                table: "MachineStatusReasons");
        }
    }
}
