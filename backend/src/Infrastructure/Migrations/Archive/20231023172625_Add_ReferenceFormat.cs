using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_ReferenceFormat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceFormatId",
                table: "References",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReferenceFormats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 250, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceFormats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_References_ReferenceFormatId",
                table: "References",
                column: "ReferenceFormatId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceFormats_Code",
                table: "ReferenceFormats",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_References_ReferenceFormats_ReferenceFormatId",
                table: "References",
                column: "ReferenceFormatId",
                principalTable: "ReferenceFormats",
                principalColumn: "Id");

            var statement = $"INSERT INTO public.\"ReferenceFormats\" (\"Id\", \"Code\", \"Description\", \"CreatedOn\", \"UpdatedOn\", \"Disabled\")" +
                "VALUES ('1a4433a0-2e30-4231-965b-95d7fa503de1'::uuid, 'RODO', 'Rodó', NOW(), NOW(), false)," +
                "       ('2a4433a0-2e30-4231-965b-95d7fa503de2'::uuid, 'TUB', 'Tub', NOW(), NOW(), false)," +
                "       ('3a4433a0-2e30-4231-965b-95d7fa503de3'::uuid, 'PLACA', 'Placa', NOW(), NOW(), false);";
            migrationBuilder.Sql(statement);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_References_ReferenceFormats_ReferenceFormatId",
                table: "References");

            migrationBuilder.DropTable(
                name: "ReferenceFormats");

            migrationBuilder.DropIndex(
                name: "IX_References_ReferenceFormatId",
                table: "References");

            migrationBuilder.DropColumn(
                name: "ReferenceFormatId",
                table: "References");

            migrationBuilder.AddColumn<int>(
                name: "FormatId",
                table: "References",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
