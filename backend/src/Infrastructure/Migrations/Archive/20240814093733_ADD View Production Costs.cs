using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ADDViewProductionCosts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.vw_productioncosts\r\n AS\r\n SELECT pp.\"Date\",\r\n    date_part('year'::text, pp.\"Date\") AS \"Year\",\r\n    date_part('month'::text, pp.\"Date\") AS \"Month\",\r\n    date_part('week'::text, pp.\"Date\") AS \"Week\",\r\n    pp.\"WorkcenterTime\",\r\n    pp.\"MachineHourCost\",\r\n    round(pp.\"WorkcenterTime\"::numeric / 60::numeric * pp.\"MachineHourCost\", 2) AS \"PartWorkcenterCost\",\r\n    pp.\"OperatorTime\",\r\n    pp.\"OperatorHourCost\",\r\n    round(pp.\"OperatorTime\"::numeric / 60::numeric * pp.\"OperatorHourCost\", 2) AS \"PartOperatorCost\",\r\n    wc.\"Name\" AS \"WorkcenterName\",\r\n    wt.\"Name\" AS \"WorkcenterTypeName\",\r\n    op.\"Code\" AS \"OperatorCode\",\r\n    concat(op.\"Name\", ' ', op.\"Surname\") AS \"OperatorName\"\r\n   FROM \"ProductionParts\" pp\r\n     JOIN \"Workcenters\" wc ON pp.\"WorkcenterId\" = wc.\"Id\"\r\n     JOIN \"WorkcenterTypes\" wt ON wc.\"WorkcenterTypeId\" = wt.\"Id\"\r\n     JOIN \"Operators\" op ON pp.\"OperatorId\" = op.\"Id\"\r\n  ORDER BY pp.\"Date\";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW public.""vw_productioncosts""");
        }
    }
}
