using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_EstimatedOperatorTimeInWmAndWoDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedOperatorTime",
                table: "WorkOrderPhaseDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0.0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedOperatorTime",
                table: "WorkMasterPhaseDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0.0m);

            // Update EstimatedOperatorTime with EstimatedTime in WorkMasterPhaseDetail and WorkOrderPhaseDetail
            var sql = $"UPDATE \"WorkMasterPhaseDetail\" SET \"EstimatedOperatorTime\" = \"EstimatedTime\";" +
                      $"UPDATE \"WorkOrderPhaseDetail\" SET \"EstimatedOperatorTime\" = \"EstimatedTime\";";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedOperatorTime",
                table: "WorkOrderPhaseDetail");

            migrationBuilder.DropColumn(
                name: "EstimatedOperatorTime",
                table: "WorkMasterPhaseDetail");
        }
    }
}
