using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateHistoricalView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                                    CREATE OR REPLACE VIEW public.vw_workcentershift_historical
                                    AS
                                    SELECT
                                        wd.""Id"",
                                        COALESCE((op.""Surname""::text || ', '::text) || op.""Name""::text, '') AS ""Operator"",
                                        wc.""Description"" AS ""Workcenter"",
                                        ms.""Name"" AS ""MachineStatus"",
                                        wd.""StartTime"",
                                        wd.""EndTime"",
                                        wd.""QuantityOk"",
                                        wd.""QuantityKo"",
                                        round(
                                            EXTRACT(epoch FROM wd.""EndTime"" - wd.""StartTime"") / 3600::numeric /
                                            CASE
                                                WHEN wd.""ConcurrentOperatorWorkcenters"" = 0 THEN 1
                                                ELSE wd.""ConcurrentOperatorWorkcenters""
                                            END::numeric,
                                            2
                                        ) AS ""TotalHours"",
                                        round(
                                            EXTRACT(epoch FROM wd.""EndTime"" - wd.""StartTime"") / 3600::numeric /
                                            CASE
                                                WHEN wd.""ConcurrentOperatorWorkcenters"" = 0 THEN 1
                                                ELSE wd.""ConcurrentOperatorWorkcenters""
                                            END::numeric * wd.""OperatorCost"",
                                            2
                                        ) AS ""OperatorCost"",
                                        round(
                                            EXTRACT(epoch FROM wd.""EndTime"" - wd.""StartTime"") / 3600::numeric /
                                            CASE
                                                WHEN wd.""ConcurrentOperatorWorkcenters"" = 0 THEN 1
                                                ELSE wd.""ConcurrentOperatorWorkcenters""
                                            END::numeric * wd.""WorkcenterCost"",
                                            2
                                        ) AS ""WorkcenterCost"",
                                        round(
                                            EXTRACT(epoch FROM wd.""EndTime"" - wd.""StartTime"") / 3600::numeric /
                                            CASE
                                                WHEN wd.""ConcurrentOperatorWorkcenters"" = 0 THEN 1
                                                ELSE wd.""ConcurrentOperatorWorkcenters""
                                            END::numeric * wd.""OperatorCost"",
                                            2
                                        ) +
                                        round(
                                            EXTRACT(epoch FROM wd.""EndTime"" - wd.""StartTime"") / 3600::numeric /
                                            CASE
                                                WHEN wd.""ConcurrentOperatorWorkcenters"" = 0 THEN 1
                                                ELSE wd.""ConcurrentOperatorWorkcenters""
                                            END::numeric * wd.""WorkcenterCost"",
                                            2
                                        ) AS ""TotalCost"",
                                        COALESCE(wo.""Code"", '') AS ""WorkOrderCode"",
                                        COALESCE(wo.""PlannedQuantity"", 0::numeric(18,4)) AS ""PlannedQuantity"",
                                        COALESCE(wo.""MachineCost"", 0::numeric(18,4)) AS ""EstimatedMachineCost"",
                                        COALESCE(wo.""MachineTime"", 0::numeric(18,4)) AS ""EstimatedMachineTime"",
                                        COALESCE(wo.""OperatorCost"", 0::numeric(18,4)) AS ""EstimatedOperatorCost"",
                                        COALESCE(wo.""OperatorTime"", 0::numeric(18,4)) AS ""EstimatedOperatorTime"",
                                        COALESCE(wp.""Code"", '') AS ""WorkOrderPhaseCode"",
                                        COALESCE(wp.""Description"", '') AS ""WorkOrderPhaseDescription"",
                                        CASE
                                            WHEN wp.""PreferredWorkcenterId"" IS NOT NULL
                                            AND ws.""WorkcenterId"" IS NOT NULL
                                            AND wp.""PreferredWorkcenterId"" = ws.""WorkcenterId""
                                            THEN true
                                            ELSE false
                                        END AS ""IsPreferredWorkcenter"",
                                        COALESCE(re.""Code"", '') AS ""ReferenceCode"",
                                        COALESCE(re.""Description"", '') AS ""ReferenceDescription"",
                                        COALESCE(cu.""ComercialName"", '') AS ""CustomerComercialName"",
                                        COALESCE(op.""Id"", '00000000-0000-0000-0000-000000000000'::uuid) AS ""OperatorId"",
                                        COALESCE(wc.""Id"", '00000000-0000-0000-0000-000000000000'::uuid) AS ""WorkcenterId"",
                                        COALESCE(re.""Id"", '00000000-0000-0000-0000-000000000000'::uuid) AS ""ReferenceId"",
                                        COALESCE(cu.""Id"", '00000000-0000-0000-0000-000000000000'::uuid) AS ""CustomerId"",
                                        COALESCE(wo.""Id"", '00000000-0000-0000-0000-000000000000'::uuid) AS ""WorkOrderId"",
                                        COALESCE(wp.""Id"", '00000000-0000-0000-0000-000000000000'::uuid) AS ""WorkOrderPhaseId""
                                    FROM data.""WorkcenterShifts"" ws
                                        JOIN data.""WorkcenterShiftDetails"" wd
                                            ON ws.""Id"" = wd.""WorkcenterShiftId""
                                            AND wd.""EndTime"" IS NOT NULL
                                        JOIN ""Workcenters"" wc ON ws.""WorkcenterId"" = wc.""Id""
                                        JOIN ""MachineStatuses"" ms ON wd.""MachineStatusId"" = ms.""Id""
                                        JOIN ""ShiftDetails"" sd ON ws.""ShiftDetailId"" = sd.""Id""
                                        JOIN ""Shifts"" sh ON sd.""ShiftId"" = sh.""Id""
                                        LEFT JOIN ""Operators"" op ON wd.""OperatorId"" = op.""Id""
                                        LEFT JOIN ""WorkOrderPhase"" wp ON wd.""WorkOrderPhaseId"" = wp.""Id""
                                        LEFT JOIN ""WorkOrder"" wo ON wp.""WorkOrderId"" = wo.""Id""
                                        LEFT JOIN ""References"" re ON wo.""ReferenceId"" = re.""Id""
                                        LEFT JOIN ""Customers"" cu ON re.""CustomerId"" = cu.""Id""
                                    ORDER BY
                                        ((op.""Surname""::text || ', '::text) || op.""Name""::text),
                                        wc.""Description"",
                                        wd.""StartTime"";
                                    ");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.vw_workcentershift_historical");
        }
    }
}
