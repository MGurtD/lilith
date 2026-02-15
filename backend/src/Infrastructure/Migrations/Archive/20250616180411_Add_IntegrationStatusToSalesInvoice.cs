using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_IntegrationStatusToSalesInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IntegrationStatusId",
                table: "SalesInvoice",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoice_IntegrationStatusId",
                table: "SalesInvoice",
                column: "IntegrationStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoice_Statuses_IntegrationStatusId",
                table: "SalesInvoice",
                column: "IntegrationStatusId",
                principalTable: "Statuses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoice_Statuses_IntegrationStatusId",
                table: "SalesInvoice");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoice_IntegrationStatusId",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "IntegrationStatusId",
                table: "SalesInvoice");
        }
    }
}
