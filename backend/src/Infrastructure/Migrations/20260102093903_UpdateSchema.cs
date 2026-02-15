using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PurchaseInvoiceStatuses_PurchaseInvoiceSta~",
                table: "PurchaseInvoices");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceStatusTransitions");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_PurchaseInvoiceStatusId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceStatusId",
                table: "PurchaseInvoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseInvoiceStatusId",
                table: "PurchaseInvoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Description = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceStatusTransitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToStatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceStatusTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceStatusTransitions_PurchaseInvoiceStatuses_Fr~",
                        column: x => x.FromStatusId,
                        principalTable: "PurchaseInvoiceStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceStatusTransitions_PurchaseInvoiceStatuses_To~",
                        column: x => x.ToStatusId,
                        principalTable: "PurchaseInvoiceStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_PurchaseInvoiceStatusId",
                table: "PurchaseInvoices",
                column: "PurchaseInvoiceStatusId");

            migrationBuilder.CreateIndex(
                name: "UK_PurchaseInvoiceStatuses_Name",
                table: "PurchaseInvoiceStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceStatusTransitions_FromStatusId",
                table: "PurchaseInvoiceStatusTransitions",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceStatusTransitions_ToStatusId",
                table: "PurchaseInvoiceStatusTransitions",
                column: "ToStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PurchaseInvoiceStatuses_PurchaseInvoiceSta~",
                table: "PurchaseInvoices",
                column: "PurchaseInvoiceStatusId",
                principalTable: "PurchaseInvoiceStatuses",
                principalColumn: "Id");
        }
    }
}
