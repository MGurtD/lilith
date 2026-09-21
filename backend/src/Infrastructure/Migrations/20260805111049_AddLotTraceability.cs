using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLotTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultProducedLotId",
                table: "WorkOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LotId",
                table: "Stocks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LotId",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LotId",
                table: "ReceiptDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LotId",
                table: "DeliveryNoteDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Lot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "varchar", maxLength: 100, nullable: false, defaultValue: ""),
                    SupplierLotCode = table.Column<string>(type: "varchar", maxLength: 100, nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RemainingQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    Comment = table.Column<string>(type: "varchar", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lot_References_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "References",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Backfill: crear un "lot buit" per cada referencia amb estoc/moviments/recepcions existents
            // i assignar-lo als registres historics, ja que el sistema no tenia concepte de lot fins ara.
            migrationBuilder.Sql(@"INSERT INTO public.""Lot"" (""Id"", ""ReferenceId"", ""Code"", ""RemainingQuantity"", ""CreatedOn"", ""UpdatedOn"", ""Disabled"")
SELECT gen_random_uuid(), refs.""ReferenceId"", '', 0, NOW(), NOW(), false
FROM (
    SELECT DISTINCT ""ReferenceId"" FROM public.""Stocks""
    UNION
    SELECT DISTINCT ""ReferenceId"" FROM public.""StockMovements""
    UNION
    SELECT DISTINCT ""ReferenceId"" FROM public.""ReceiptDetails""
) refs");

            migrationBuilder.Sql(@"UPDATE public.""Stocks"" s
SET ""LotId"" = l.""Id""
FROM public.""Lot"" l
WHERE s.""LotId"" IS NULL AND l.""ReferenceId"" = s.""ReferenceId"" AND l.""Code"" = ''");

            migrationBuilder.Sql(@"UPDATE public.""StockMovements"" sm
SET ""LotId"" = l.""Id""
FROM public.""Lot"" l
WHERE sm.""LotId"" IS NULL AND l.""ReferenceId"" = sm.""ReferenceId"" AND l.""Code"" = ''");

            migrationBuilder.Sql(@"UPDATE public.""ReceiptDetails"" rd
SET ""LotId"" = l.""Id""
FROM public.""Lot"" l
WHERE rd.""LotId"" IS NULL AND l.""ReferenceId"" = rd.""ReferenceId"" AND l.""Code"" = ''");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_DefaultProducedLotId",
                table: "WorkOrder",
                column: "DefaultProducedLotId");

            migrationBuilder.CreateIndex(
                name: "idx_Location_Reference_Lot",
                table: "Stocks",
                columns: new[] { "LocationId", "ReferenceId", "LotId" });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_LotId",
                table: "Stocks",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "idx_lotid",
                table: "StockMovements",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptDetails_LotId",
                table: "ReceiptDetails",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteDetails_LotId",
                table: "DeliveryNoteDetails",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_Lot_ReferenceId_Code",
                table: "Lot",
                columns: new[] { "ReferenceId", "Code" },
                unique: true,
                filter: "\"ClosedDate\" IS NULL AND \"Code\" <> ''");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNoteDetails_Lot_LotId",
                table: "DeliveryNoteDetails",
                column: "LotId",
                principalTable: "Lot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptDetails_Lot_LotId",
                table: "ReceiptDetails",
                column: "LotId",
                principalTable: "Lot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Lot_LotId",
                table: "StockMovements",
                column: "LotId",
                principalTable: "Lot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Lot_LotId",
                table: "Stocks",
                column: "LotId",
                principalTable: "Lot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_Lot_DefaultProducedLotId",
                table: "WorkOrder",
                column: "DefaultProducedLotId",
                principalTable: "Lot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNoteDetails_Lot_LotId",
                table: "DeliveryNoteDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptDetails_Lot_LotId",
                table: "ReceiptDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Lot_LotId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Lot_LotId",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_Lot_DefaultProducedLotId",
                table: "WorkOrder");

            migrationBuilder.DropTable(
                name: "Lot");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_DefaultProducedLotId",
                table: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "idx_Location_Reference_Lot",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_LotId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "idx_lotid",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptDetails_LotId",
                table: "ReceiptDetails");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryNoteDetails_LotId",
                table: "DeliveryNoteDetails");

            migrationBuilder.DropColumn(
                name: "DefaultProducedLotId",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "LotId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "LotId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "LotId",
                table: "ReceiptDetails");

            migrationBuilder.DropColumn(
                name: "LotId",
                table: "DeliveryNoteDetails");
        }
    }
}
