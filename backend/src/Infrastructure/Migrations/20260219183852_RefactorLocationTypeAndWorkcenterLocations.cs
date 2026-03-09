using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorLocationTypeAndWorkcenterLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationType",
                table: "Locations",
                type: "varchar",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkcenterLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkcenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkcenterLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkcenterLocations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkcenterLocations_Workcenters_WorkcenterId",
                        column: x => x.WorkcenterId,
                        principalTable: "Workcenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkcenterLocations_LocationId",
                table: "WorkcenterLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "UK_WorkcenterLocation",
                table: "WorkcenterLocations",
                columns: new[] { "WorkcenterId", "LocationId" });

            // Step 3: Data migration - create supply locations for all active workcenters
            migrationBuilder.Sql(@"
                INSERT INTO ""Locations"" (""Id"", ""Name"", ""Description"", ""WarehouseId"", ""LocationType"", ""CreatedOn"", ""UpdatedOn"", ""Disabled"")
                SELECT
                    uuid_generate_v4(),
                    'APR-' || wc.""Name"",
                    'Ubicació d''aprovisionament per ' || wc.""Name"",
                    (SELECT w.""Id"" FROM ""Warehouses"" w WHERE w.""Disabled"" = false LIMIT 1),
                    'Supply',
                    NOW(),
                    NOW(),
                    false
                FROM ""Workcenters"" wc
                WHERE wc.""Disabled"" = false
                  AND NOT EXISTS (SELECT 1 FROM ""Locations"" l WHERE l.""Name"" = 'APR-' || wc.""Name"")
                  AND (SELECT w.""Id"" FROM ""Warehouses"" w WHERE w.""Disabled"" = false LIMIT 1) IS NOT NULL;
            ");

            // Step 4: Data migration - link workcenters to their supply locations via WorkcenterLocations
            migrationBuilder.Sql(@"
                INSERT INTO ""WorkcenterLocations"" (""Id"", ""WorkcenterId"", ""LocationId"", ""CreatedOn"", ""UpdatedOn"", ""Disabled"")
                SELECT uuid_generate_v4(), wc.""Id"", l.""Id"", NOW(), NOW(), false
                FROM ""Workcenters"" wc
                JOIN ""Locations"" l ON l.""Name"" = 'APR-' || wc.""Name""
                WHERE wc.""Disabled"" = false;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkcenterLocations");

            // Remove APR- supply locations created by this migration
            migrationBuilder.Sql("DELETE FROM \"Locations\" WHERE \"Name\" LIKE 'APR-%';");

            migrationBuilder.DropColumn(
                name: "LocationType",
                table: "Locations");
        }
    }
}
