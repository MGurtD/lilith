using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DistanceFromSite",
                table: "CustomerAddress",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "TransportRate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransportRateDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportRateId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinWeight = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    MaxWeight = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    MinVolume = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    MaxVolume = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    MinDistance = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    MaxDistance = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    Price = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportRateDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransportRateDetail_TransportRate_TransportRateId",
                        column: x => x.TransportRateId,
                        principalTable: "TransportRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UK_TransportRate_Name_Dates",
                table: "TransportRate",
                columns: new[] { "Name", "ValidFrom", "ValidTo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportRateDetail_TransportRateId",
                table: "TransportRateDetail",
                column: "TransportRateId");

            migrationBuilder.Sql(@"INSERT INTO ""SupplierTypes"" (""Id"", ""Name"", ""Description"", ""CreatedOn"", ""UpdatedOn"", ""Disabled"")
	VALUES (uuid_generate_v4(), 'Logistica', 'Logistica', NOW(), NOW(), false);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""SupplierTypes"" WHERE ""Name"" = 'Logistica';");

            migrationBuilder.DropTable(
                name: "TransportRateDetail");

            migrationBuilder.DropTable(
                name: "TransportRate");

            migrationBuilder.DropColumn(
                name: "DistanceFromSite",
                table: "CustomerAddress");
        }
    }
}
