using Application.Contracts;
using Domain.Entities.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Warehouse
{
    public class LotRepository(ApplicationDbContext context) : Repository<Lot, Guid>(context), ILotRepository
    {
        // Max recursion depth safeguard against corrupt data / cycles (e.g. a WorkOrder wrongly pointing back to one of its own inputs).
        private const int MAX_DEPTH = 10;

        // BACKWARD: from a produced/sold lot, walk down towards its purchase origin(s).
        // Edges: WorkOrder.DefaultProducedLotId (produced lot) -> StockMovement CONSUMPTION LotId (consumed lot), via WorkOrderPhase.
        // A lot with its own ReceiptDetail is a base case (purchase origin) and is not expanded further.
        public async Task<List<LotTraceabilityEdgeRow>> GetBackwardTraceabilityEdges(Guid lotId)
        {
            const string sql = """
                WITH RECURSIVE backward_edges AS (
                    SELECT
                        wo."DefaultProducedLotId" AS produced_lot_id,
                        sm."LotId" AS consumed_lot_id,
                        SUM(ABS(sm."Quantity")) AS quantity
                    FROM "WorkOrder" wo
                    JOIN "WorkOrderPhase" wop ON wop."WorkOrderId" = wo."Id"
                    JOIN "StockMovements" sm
                        ON sm."Entity" = 'WorkOrderPhase'
                       AND sm."EntityId" = wop."Id"
                       AND sm."MovementType" = 'CONSUMPTION'
                       AND sm."Quantity" < 0
                       AND sm."LotId" IS NOT NULL
                    WHERE wo."DefaultProducedLotId" IS NOT NULL
                    GROUP BY wo."DefaultProducedLotId", sm."LotId"
                ),
                chain AS (
                    SELECT
                        CAST(NULL AS uuid) AS "ParentLotId",
                        l."Id" AS "LotId",
                        0 AS "Depth",
                        CAST(l."RemainingQuantity" AS numeric) AS "Quantity"
                    FROM "Lot" l
                    WHERE l."Id" = {0}

                    UNION ALL

                    SELECT
                        c."LotId" AS "ParentLotId",
                        e.consumed_lot_id AS "LotId",
                        c."Depth" + 1 AS "Depth",
                        CAST(e.quantity AS numeric) AS "Quantity"
                    FROM chain c
                    JOIN backward_edges e ON e.produced_lot_id = c."LotId"
                    WHERE c."Depth" < {1}
                      AND NOT EXISTS (SELECT 1 FROM "ReceiptDetails" rd WHERE rd."LotId" = c."LotId")
                )
                SELECT
                    chain."ParentLotId",
                    chain."LotId",
                    chain."Depth",
                    chain."Quantity",
                    l."Code" AS "LotCode",
                    l."ReferenceId",
                    r."Code" AS "ReferenceCode",
                    r."Description" AS "ReferenceDescription",
                    rcpt."Id" AS "ReceiptId",
                    rcpt."Number" AS "ReceiptNumber",
                    rcpt."Date" AS "ReceiptDate",
                    sup."Id" AS "SupplierId",
                    sup."ComercialName" AS "SupplierName"
                FROM chain
                JOIN "Lot" l ON l."Id" = chain."LotId"
                JOIN "References" r ON r."Id" = l."ReferenceId"
                LEFT JOIN "ReceiptDetails" rd ON rd."LotId" = chain."LotId"
                LEFT JOIN "Receipts" rcpt ON rcpt."Id" = rd."ReceiptId"
                LEFT JOIN "Suppliers" sup ON sup."Id" = rcpt."SupplierId"
                ORDER BY chain."Depth"
                """;

            return await context.Database
                .SqlQueryRaw<LotTraceabilityEdgeRow>(sql, lotId, MAX_DEPTH)
                .ToListAsync();
        }

        // FORWARD: from a purchased lot, walk up towards everything manufactured and sold from it.
        // Edges: StockMovement CONSUMPTION LotId (consumed lot) -> WorkOrder.DefaultProducedLotId (produced lot), via WorkOrderPhase.
        public async Task<List<LotTraceabilityEdgeRow>> GetForwardTraceabilityEdges(Guid lotId)
        {
            const string sql = """
                WITH RECURSIVE forward_edges AS (
                    SELECT
                        sm."LotId" AS consumed_lot_id,
                        wo."DefaultProducedLotId" AS produced_lot_id,
                        SUM(ABS(sm."Quantity")) AS quantity
                    FROM "StockMovements" sm
                    JOIN "WorkOrderPhase" wop
                        ON sm."Entity" = 'WorkOrderPhase'
                       AND sm."EntityId" = wop."Id"
                    JOIN "WorkOrder" wo ON wo."Id" = wop."WorkOrderId"
                    WHERE sm."MovementType" = 'CONSUMPTION'
                      AND sm."Quantity" < 0
                      AND sm."LotId" IS NOT NULL
                      AND wo."DefaultProducedLotId" IS NOT NULL
                    GROUP BY sm."LotId", wo."DefaultProducedLotId"
                ),
                chain AS (
                    SELECT
                        CAST(NULL AS uuid) AS "ParentLotId",
                        l."Id" AS "LotId",
                        0 AS "Depth",
                        CAST(l."RemainingQuantity" AS numeric) AS "Quantity"
                    FROM "Lot" l
                    WHERE l."Id" = {0}

                    UNION ALL

                    SELECT
                        c."LotId" AS "ParentLotId",
                        e.produced_lot_id AS "LotId",
                        c."Depth" + 1 AS "Depth",
                        CAST(e.quantity AS numeric) AS "Quantity"
                    FROM chain c
                    JOIN forward_edges e ON e.consumed_lot_id = c."LotId"
                    WHERE c."Depth" < {1}
                )
                SELECT
                    chain."ParentLotId",
                    chain."LotId",
                    chain."Depth",
                    chain."Quantity",
                    l."Code" AS "LotCode",
                    l."ReferenceId",
                    r."Code" AS "ReferenceCode",
                    r."Description" AS "ReferenceDescription",
                    CAST(NULL AS uuid) AS "ReceiptId",
                    CAST(NULL AS varchar) AS "ReceiptNumber",
                    CAST(NULL AS timestamp) AS "ReceiptDate",
                    CAST(NULL AS uuid) AS "SupplierId",
                    CAST(NULL AS varchar) AS "SupplierName"
                FROM chain
                JOIN "Lot" l ON l."Id" = chain."LotId"
                JOIN "References" r ON r."Id" = l."ReferenceId"
                ORDER BY chain."Depth"
                """;

            return await context.Database
                .SqlQueryRaw<LotTraceabilityEdgeRow>(sql, lotId, MAX_DEPTH)
                .ToListAsync();
        }
    }
}
