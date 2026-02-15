using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class create_view_detailedworkorder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.vw_detailedworkorder\r\n AS\r\n SELECT wo.\"Id\" AS \"WorkOrderId\",\r\n    wo.\"Code\" AS \"WorkOrderCode\",\r\n    st.\"Name\" AS \"WorkOrderStatusCode\",\r\n    st.\"Description\" AS \"WorkOrderStatusDescription\",\r\n    wo.\"PlannedQuantity\",\r\n    wo.\"StartTime\" AS \"WorkOrderStartTime\",\r\n    wo.\"EndTime\" AS \"WorkOrderEndTime\",\r\n    wo.\"Order\" AS \"WorkOrderOrder\",\r\n    wo.\"Comment\" AS \"WorkOrderComment\",\r\n    wo.\"PlannedDate\",\r\n    re.\"Code\" AS \"ReferenceCode\",\r\n    re.\"Description\" AS \"ReferenceDescription\",\r\n    re.\"Version\" AS \"ReferenceVersion\",\r\n    re.\"Cost\" AS \"ReferenceCost\",\r\n    wp.\"Id\" AS \"WorkOrderPhaseId\",\r\n    wp.\"Code\" AS \"WorkOrderPhaseCode\",\r\n    wp.\"Description\" AS \"WorkOrderPhaseDescription\",\r\n    wp.\"Comment\" AS \"WorkOrderPhaseComment\",\r\n    stwp.\"Name\" AS \"WorkOrderPhaseStatusCode\",\r\n    stwp.\"Description\" AS \"WorkOrderPhaseStatusDescription\",\r\n    wp.\"StartTime\" AS \"WorkOrderPhaseStartTime\",\r\n    wp.\"EndTime\" AS \"WorkOrderPhaseEndTime\",\r\n    wd.\"Id\" AS \"WorkOrderPhaseDetailId\",\r\n    wd.\"Order\" AS \"WorkOrderPhaseDetailOrder\",\r\n    wd.\"EstimatedTime\" AS \"WorkOrderPhaseDetailEstimatedTime\",\r\n    wd.\"Comment\" AS \"WorkOrderPhaseDetailComment\",\r\n    ms.\"Name\" AS \"MachineStatusName\",\r\n    ms.\"Description\" AS \"MachineStatusDescription\",\r\n    wc.\"Id\" AS \"WorkcenterId\",\r\n    wc.\"Name\" AS \"WorkcenterName\",\r\n    wc.\"Description\" AS \"WorkcenterDescription\",\r\n    wc.\"costHour\" AS \"WorkcenterCost\",\r\n        CASE\r\n            WHEN wp.\"PreferredWorkcenterId\" = wc.\"Id\" THEN true\r\n            ELSE false\r\n        END AS \"PreferredWorkcenter\"\r\n   FROM \"WorkOrder\" wo\r\n     JOIN \"Statuses\" st ON wo.\"StatusId\" = st.\"Id\"\r\n     JOIN \"References\" re ON wo.\"ReferenceId\" = re.\"Id\"\r\n     JOIN \"WorkOrderPhase\" wp ON wo.\"Id\" = wp.\"WorkOrderId\"\r\n     JOIN \"Statuses\" stwp ON wp.\"StatusId\" = stwp.\"Id\"\r\n     JOIN \"WorkOrderPhaseDetail\" wd ON wp.\"Id\" = wd.\"WorkOrderPhaseId\"\r\n     JOIN \"Workcenters\" wc ON wc.\"WorkcenterTypeId\" = wp.\"WorkcenterTypeId\"\r\n     JOIN \"MachineStatuses\" ms ON wd.\"MachineStatusId\" = ms.\"Id\";\r\n\r\nALTER TABLE public.vw_detailedworkorder\r\n    OWNER TO ubuntu;\r\n");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.vw_detailedworkorder;");
        }
    }
}
