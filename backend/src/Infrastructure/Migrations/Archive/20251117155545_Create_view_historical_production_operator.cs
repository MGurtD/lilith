using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_view_historical_production_operator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE VIEW vw_workcentershift_historical_operator\r\nAS\r\nSELECT \twd.\"Id\" as \"Id\",\r\n\t\top.\"Surname\" || ', ' || op.\"Name\" as \"Key\",\r\n\t\twc.\"Description\" as \"Workcenter\",\r\n\t\tms.\"Name\" as \"MachineStatus\",\r\n\t\twd.\"StartTime\",\r\n\t\twd.\"EndTime\",\r\n\t\twd.\"QuantityOk\",\r\n\t\twd.\"QuantityKo\",\r\n\t\tROUND((EXTRACT(EPOCH FROM (wd.\"EndTime\" - wd.\"StartTime\")) / 3600)/ CASE WHEN wd.\"ConcurrentOperatorWorkcenters\" = 0 THEN 1 ELSE wd.\"ConcurrentOperatorWorkcenters\" END,2) as \"TotalHours\",\r\n\t\tROUND(((EXTRACT(EPOCH FROM (wd.\"EndTime\" - wd.\"StartTime\")) / 3600)/ CASE WHEN wd.\"ConcurrentOperatorWorkcenters\" = 0 THEN 1 ELSE wd.\"ConcurrentOperatorWorkcenters\" END)* wd.\"OperatorCost\",2) AS \"OperatorCost\",\r\n\t\tROUND(((EXTRACT(EPOCH FROM (wd.\"EndTime\" - wd.\"StartTime\")) / 3600)/ CASE WHEN wd.\"ConcurrentOperatorWorkcenters\" = 0 THEN 1 ELSE wd.\"ConcurrentOperatorWorkcenters\" END)* wd.\"WorkcenterCost\",2) AS \"WorkcenterCost\",\r\n\t\tROUND(((EXTRACT(EPOCH FROM (wd.\"EndTime\" - wd.\"StartTime\")) / 3600)/ CASE WHEN wd.\"ConcurrentOperatorWorkcenters\" = 0 THEN 1 ELSE wd.\"ConcurrentOperatorWorkcenters\" END)* wd.\"OperatorCost\",2) +\r\n\t\tROUND(((EXTRACT(EPOCH FROM (wd.\"EndTime\" - wd.\"StartTime\")) / 3600)/ CASE WHEN wd.\"ConcurrentOperatorWorkcenters\" = 0 THEN 1 ELSE wd.\"ConcurrentOperatorWorkcenters\" END)* wd.\"WorkcenterCost\",2) AS \"TotalCost\"\t\t\r\nFROM data.\"WorkcenterShifts\" ws\r\n\tINNER JOIN data.\"WorkcenterShiftDetails\" wd\r\n\t\tON ws.\"Id\" = wd.\"WorkcenterShiftId\"\r\n\tINNER JOIN public.\"Workcenters\" wc\r\n\t\tON ws.\"WorkcenterId\" = wc.\"Id\"\r\n\tINNER JOIN public.\"MachineStatuses\" ms\r\n\t\tON wd.\"MachineStatusId\" = ms.\"Id\"\r\n\tINNER JOIN public.\"ShiftDetails\" sd\r\n\t\tON ws.\"ShiftDetailId\" = sd.\"Id\"\r\n\tINNER JOIN public.\"Shifts\" sh\r\n\t\tON sd.\"ShiftId\" = sh.\"Id\"\r\n\tLEFT JOIN public.\"Operators\" op\r\n\t\tON wd.\"OperatorId\" = op.\"Id\"\t\r\nORDER BY 2,3,5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW vw_workcentershift_historical_operator;");
        }
    }
}
