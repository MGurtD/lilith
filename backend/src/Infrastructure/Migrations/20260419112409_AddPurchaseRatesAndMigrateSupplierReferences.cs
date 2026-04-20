using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseRatesAndMigrateSupplierReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseRate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRate_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRateDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseRateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    From = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    To = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CalculationType = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRateDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRateDetail_PurchaseRate_PurchaseRateId",
                        column: x => x.PurchaseRateId,
                        principalTable: "PurchaseRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseRateDetail_References_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "References",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRate_SupplierId",
                table: "PurchaseRate",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRateDetail_PurchaseRateId",
                table: "PurchaseRateDetail",
                column: "PurchaseRateId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRateDetail_ReferenceId",
                table: "PurchaseRateDetail",
                column: "ReferenceId");

            // Migrar dades: crear una PurchaseRate "Tarifa 2026" per cada proveïdor que tingui SupplierReferences amb preu
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    supplier_row RECORD;
                    new_rate_id UUID;
                    now_ts TIMESTAMPTZ := NOW();
                BEGIN
                    -- Per cada proveïdor distinct amb referències amb preu
                    FOR supplier_row IN
                        SELECT DISTINCT ""SupplierId""
                        FROM ""SupplierReferences""
                        WHERE ""SupplierPrice"" > 0 AND ""Disabled"" = false
                    LOOP
                        new_rate_id := gen_random_uuid();

                        -- Crear la capçalera PurchaseRate
                        INSERT INTO ""PurchaseRate"" (""Id"", ""Name"", ""SupplierId"", ""ValidFrom"", ""ValidTo"", ""CreatedOn"", ""UpdatedOn"", ""Disabled"")
                        VALUES (
                            new_rate_id,
                            'Tarifa 2026',
                            supplier_row.""SupplierId"",
                            '2026-01-01',
                            '2026-12-31',
                            now_ts,
                            now_ts,
                            false
                        );

                        -- Crear els detalls PurchaseRateDetail per totes les referències del proveïdor
                        INSERT INTO ""PurchaseRateDetail"" (""Id"", ""PurchaseRateId"", ""ReferenceId"", ""From"", ""To"", ""CalculationType"", ""Price"", ""CreatedOn"", ""UpdatedOn"", ""Disabled"")
                        SELECT
                            gen_random_uuid(),
                            new_rate_id,
                            ""ReferenceId"",
                            0,
                            999999,
                            2, -- Units
                            ""SupplierPrice"",
                            now_ts,
                            now_ts,
                            false
                        FROM ""SupplierReferences""
                        WHERE ""SupplierId"" = supplier_row.""SupplierId""
                          AND ""SupplierPrice"" > 0
                          AND ""Disabled"" = false;
                    END LOOP;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseRateDetail");

            migrationBuilder.DropTable(
                name: "PurchaseRate");
        }
    }
}
