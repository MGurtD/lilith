using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderTransportAndExternalServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesOrderExternalServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderExternalServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderExternalServices_SalesOrderHeader_SalesOrderHeade~",
                        column: x => x.SalesOrderHeaderId,
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderTransports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportRateDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Distance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Destination = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderTransports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderTransports_SalesOrderHeader_SalesOrderHeaderId",
                        column: x => x.SalesOrderHeaderId,
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderExternalServiceDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderExternalServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderExternalServiceDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderExternalServiceDetails_SalesOrderDetail_SalesOrde~",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderExternalServiceDetails_SalesOrderExternalServices~",
                        column: x => x.SalesOrderExternalServiceId,
                        principalTable: "SalesOrderExternalServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderExternalServiceDetails_SalesOrderDetailId",
                table: "SalesOrderExternalServiceDetails",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderExternalServiceDetails_SalesOrderExternalServiceId",
                table: "SalesOrderExternalServiceDetails",
                column: "SalesOrderExternalServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderExternalServices_SalesOrderHeaderId",
                table: "SalesOrderExternalServices",
                column: "SalesOrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderTransports_SalesOrderHeaderId",
                table: "SalesOrderTransports",
                column: "SalesOrderHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesOrderExternalServiceDetails");

            migrationBuilder.DropTable(
                name: "SalesOrderTransports");

            migrationBuilder.DropTable(
                name: "SalesOrderExternalServices");
        }
    }
}
