using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Migrations
{
    public partial class Add_VerifactuRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FinalStatusId",
                table: "Lifecycles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesInvoiceVerifactuRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesInvoiceId = table.Column<Guid>(type: "uuid", maxLength: 512, nullable: false),
                    Hash = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    Request = table.Column<string>(type: "text", nullable: false),
                    Response = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "varchar", maxLength: 512, nullable: true),
                    TimestampResponse = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    Disabled = table.Column<bool>(type: "bool", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceVerifactuRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceVerifactuRequest_SalesInvoice_SalesInvoiceId",
                        column: x => x.SalesInvoiceId,
                        principalTable: "SalesInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceVerifactuRequest_SalesInvoiceId",
                table: "SalesInvoiceVerifactuRequest",
                column: "SalesInvoiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesInvoiceVerifactuRequest");

            migrationBuilder.DropColumn(
                name: "FinalStatusId",
                table: "Lifecycles");
        }
    }
}
