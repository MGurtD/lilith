using Application.Contracts;
using Domain.Entities.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Warehouse;

public class LotTraceabilityService(IUnitOfWork unitOfWork) : ILotTraceabilityService
{
    public async Task<LotBackwardTraceabilityDto?> GetBackwardTraceability(Guid lotId)
    {
        var edges = await unitOfWork.Lots.GetBackwardTraceabilityEdges(lotId);
        if (edges.Count == 0) return null; // Anchor row absent => lot does not exist

        var rootRow = edges.First(e => e.Depth == 0);
        var root = BuildBackwardNode(rootRow, edges);
        await AttachMovements(root);
        await AttachPurchaseOrigins(root);

        return new LotBackwardTraceabilityDto
        {
            LotId = rootRow.LotId,
            LotCode = rootRow.LotCode,
            ReferenceId = rootRow.ReferenceId,
            ReferenceCode = rootRow.ReferenceCode,
            ReferenceDescription = rootRow.ReferenceDescription,
            Root = root
        };
    }

    public async Task<LotForwardTraceabilityDto?> GetForwardTraceability(Guid lotId)
    {
        var edges = await unitOfWork.Lots.GetForwardTraceabilityEdges(lotId);
        if (edges.Count == 0) return null; // Anchor row absent => lot does not exist

        var rootRow = edges.First(e => e.Depth == 0);
        var childrenByParent = edges.Where(e => e.ParentLotId.HasValue).ToLookup(e => e.ParentLotId!.Value);
        var root = BuildForwardNode(rootRow, childrenByParent);

        // A node is a final product (nothing left to trace forward) when it has no children in the chain.
        var leafLotIds = CollectLeafLotIds(root).Distinct().ToList();
        var salesByLot = await GetSalesDestinationsByLot(leafLotIds);
        AttachSalesDestinations(root, salesByLot);
        await AttachMovements(root);

        return new LotForwardTraceabilityDto
        {
            LotId = rootRow.LotId,
            LotCode = rootRow.LotCode,
            ReferenceId = rootRow.ReferenceId,
            ReferenceCode = rootRow.ReferenceCode,
            ReferenceDescription = rootRow.ReferenceDescription,
            Root = root
        };
    }

    public async Task<LotRecallReportDto?> GetRecallReport(Guid lotId)
    {
        var forward = await GetForwardTraceability(lotId);
        if (forward == null) return null;

        var destinations = new List<SalesDestinationDto>();
        CollectSalesDestinations(forward.Root, destinations);

        var affectedCustomers = destinations
            .GroupBy(d => new { d.CustomerId, d.CustomerName })
            .Select(g => new RecallCustomerDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.CustomerName,
                DeliveryNotes = g
                    .Select(d => new RecallDeliveryNoteDto
                    {
                        DeliveryNoteId = d.DeliveryNoteId,
                        DeliveryNoteNumber = d.DeliveryNoteNumber,
                        DeliveryDate = d.DeliveryDate,
                        LotId = d.LotId,
                        LotCode = d.LotCode,
                        ReferenceId = d.ReferenceId,
                        ReferenceCode = d.ReferenceCode,
                        ReferenceDescription = d.ReferenceDescription,
                        Quantity = d.Quantity
                    })
                    .OrderBy(dn => dn.DeliveryDate)
                    .ToList()
            })
            .OrderBy(c => c.CustomerName)
            .ToList();

        return new LotRecallReportDto
        {
            LotId = forward.LotId,
            LotCode = forward.LotCode,
            ReferenceId = forward.ReferenceId,
            ReferenceCode = forward.ReferenceCode,
            ReferenceDescription = forward.ReferenceDescription,
            TotalAffectedDeliveryNotes = destinations.Select(d => d.DeliveryNoteId).Distinct().Count(),
            TotalAffectedQuantity = destinations.Sum(d => d.Quantity),
            AffectedCustomers = affectedCustomers
        };
    }

    // Builds one node per edge occurrence (not deduplicated by LotId), so the same lot appearing
    // through two different chains keeps its own correct per-edge quantity instead of sharing state.
    // A lot whose own ReceiptDetail(s) make it a purchase origin has no further backward_edges rows
    // (the recursive CTE already stopped there), so it naturally ends up with no children.
    private static LotTraceabilityNode BuildBackwardNode(LotTraceabilityEdgeRow row, List<LotTraceabilityEdgeRow> edges)
    {
        var node = new LotTraceabilityNode
        {
            LotId = row.LotId,
            LotCode = row.LotCode,
            ReferenceId = row.ReferenceId,
            ReferenceCode = row.ReferenceCode,
            ReferenceDescription = row.ReferenceDescription,
            Quantity = row.Quantity
        };

        foreach (var childRow in edges.Where(e => e.ParentLotId == row.LotId))
        {
            node.Children.Add(BuildBackwardNode(childRow, edges));
        }

        return node;
    }

    private static LotTraceabilityNode BuildForwardNode(LotTraceabilityEdgeRow row, ILookup<Guid, LotTraceabilityEdgeRow> childrenByParent)
    {
        var node = new LotTraceabilityNode
        {
            LotId = row.LotId,
            LotCode = row.LotCode,
            ReferenceId = row.ReferenceId,
            ReferenceCode = row.ReferenceCode,
            ReferenceDescription = row.ReferenceDescription,
            Quantity = row.Quantity
        };

        foreach (var childRow in childrenByParent[row.LotId])
        {
            node.Children.Add(BuildForwardNode(childRow, childrenByParent));
        }

        return node;
    }

    private static IEnumerable<Guid> CollectLeafLotIds(LotTraceabilityNode node)
    {
        if (node.Children.Count == 0)
        {
            yield return node.LotId;
            yield break;
        }

        foreach (var child in node.Children)
        {
            foreach (var lotId in CollectLeafLotIds(child))
            {
                yield return lotId;
            }
        }
    }

    private static void AttachSalesDestinations(LotTraceabilityNode node, Dictionary<Guid, List<SalesDestinationDto>> salesByLot)
    {
        if (node.Children.Count == 0)
        {
            if (salesByLot.TryGetValue(node.LotId, out var sales))
            {
                foreach (var sale in sales) sale.LotCode = node.LotCode;
                node.SalesDestinations = sales;
            }
            return;
        }

        foreach (var child in node.Children)
        {
            AttachSalesDestinations(child, salesByLot);
        }
    }

    private static void CollectSalesDestinations(LotTraceabilityNode node, List<SalesDestinationDto> accumulator)
    {
        accumulator.AddRange(node.SalesDestinations);
        foreach (var child in node.Children)
        {
            CollectSalesDestinations(child, accumulator);
        }
    }

    // Final products are sold via DeliveryNoteDetail.LotId directly; StockMovement OUTPUT rows for
    // deliveries do not carry Entity/EntityId today, so this direct link is the only reliable path.
    private async Task<Dictionary<Guid, List<SalesDestinationDto>>> GetSalesDestinationsByLot(List<Guid> lotIds)
    {
        if (lotIds.Count == 0) return [];

        var details = await unitOfWork.DeliveryNotes.Details.FindAsyncWithQueryParams(
            d => d.LotId.HasValue && lotIds.Contains(d.LotId.Value),
            q => q.Include(d => d.DeliveryNote).ThenInclude(dn => dn!.Customer)
                  .Include(d => d.Reference));

        return details
            .Where(d => d.LotId.HasValue && d.DeliveryNote != null && d.DeliveryNote.Customer != null)
            .GroupBy(d => d.LotId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(d => new SalesDestinationDto
                {
                    LotId = d.LotId!.Value,
                    LotCode = string.Empty, // filled by caller from the tree node it belongs to
                    ReferenceId = d.ReferenceId,
                    ReferenceCode = d.Reference?.Code ?? string.Empty,
                    ReferenceDescription = d.Reference?.Description ?? d.Description,
                    Quantity = d.Quantity,
                    CustomerId = d.DeliveryNote!.CustomerId,
                    CustomerName = d.DeliveryNote.Customer!.ComercialName,
                    DeliveryNoteId = d.DeliveryNoteId,
                    DeliveryNoteNumber = d.DeliveryNote.Number,
                    DeliveryDate = d.DeliveryNote.DeliveryDate
                }).ToList());
    }

    // Attaches every ReceiptDetail tied to each lot in the tree. A lot can legitimately have been
    // replenished across multiple receipts, so all of them are shown rather than picking just one.
    // Only receipt lines actually moved into warehouse stock (StockMovementId set) count as origins.
    private async Task AttachPurchaseOrigins(LotTraceabilityNode root)
    {
        var lotIds = CollectAllLotIds(root).Distinct().ToList();
        if (lotIds.Count == 0) return;

        var details = await unitOfWork.Receipts.Details.FindAsyncWithQueryParams(
            d => d.LotId.HasValue && d.StockMovementId.HasValue && lotIds.Contains(d.LotId.Value),
            q => q.Include(d => d.Receipt).ThenInclude(r => r!.Supplier));

        var originsByLot = details
            .Where(d => d.LotId.HasValue && d.Receipt != null && d.Receipt.Supplier != null)
            .GroupBy(d => d.LotId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(d => new PurchaseOriginDto
                {
                    LotId = d.LotId!.Value,
                    Quantity = d.Quantity,
                    SupplierId = d.Receipt!.SupplierId,
                    SupplierName = d.Receipt.Supplier!.ComercialName,
                    ReceiptId = d.ReceiptId,
                    ReceiptNumber = d.Receipt.Number,
                    ReceiptDate = d.Receipt.Date
                }).ToList());

        AttachPurchaseOriginsToNodes(root, originsByLot);
    }

    private static void AttachPurchaseOriginsToNodes(LotTraceabilityNode node, Dictionary<Guid, List<PurchaseOriginDto>> originsByLot)
    {
        if (originsByLot.TryGetValue(node.LotId, out var origins))
        {
            foreach (var origin in origins)
            {
                origin.LotCode = node.LotCode;
                origin.ReferenceCode = node.ReferenceCode;
                origin.ReferenceDescription = node.ReferenceDescription;
            }
            node.PurchaseOrigins = origins;
        }

        foreach (var child in node.Children)
            AttachPurchaseOriginsToNodes(child, originsByLot);
    }

    // Attaches the stock movements (consumption, production and purchase/sale-derived transfers) of every lot in the tree.
    // Internal WorkOrderPhase supply/return transfers are excluded: they only relocate stock between locations
    // (always an OUTPUT+INPUT pair per action) without representing a meaningful traceability event, and showing
    // them made the tree look like it had duplicated movements.
    private async Task AttachMovements(LotTraceabilityNode root)
    {
        var lotIds = CollectAllLotIds(root).Distinct().ToList();
        var movements = await unitOfWork.StockMovements.GetByLotIds(lotIds);
        var relevantMovements = movements.Where(m => m.LotId.HasValue && IsRelevantForTraceability(m)).ToList();
        var partnerByMovementId = await GetPartnerInfoByMovementId(relevantMovements);

        var movementsByLot = relevantMovements
            .GroupBy(m => m.LotId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(m =>
                {
                    partnerByMovementId.TryGetValue(m.Id, out var partner);
                    return new LotStockMovementDto
                    {
                        MovementId = m.Id,
                        MovementType = m.MovementType,
                        Quantity = m.Quantity,
                        MovementDate = m.MovementDate,
                        Description = m.Description,
                        LocationId = m.LocationId,
                        LocationName = m.Location?.Name ?? string.Empty,
                        Entity = m.Entity,
                        EntityId = m.EntityId,
                        PartnerName = partner.PartnerName,
                        DocumentNumber = partner.DocumentNumber
                    };
                }).ToList());

        AttachMovementsToNodes(root, movementsByLot);
    }

    // Purchases link back via ReceiptDetail.StockMovementId; sales link back via the movement's own Entity/EntityId.
    private async Task<Dictionary<Guid, (string? PartnerName, string? DocumentNumber)>> GetPartnerInfoByMovementId(List<StockMovement> movements)
    {
        var result = new Dictionary<Guid, (string?, string?)>();
        var movementIds = movements.Select(m => m.Id).ToList();
        if (movementIds.Count == 0) return result;

        var receiptDetails = await unitOfWork.Receipts.Details.FindAsyncWithQueryParams(
            d => d.StockMovementId.HasValue && movementIds.Contains(d.StockMovementId.Value),
            q => q.Include(d => d.Receipt).ThenInclude(r => r!.Supplier));

        foreach (var d in receiptDetails.Where(d => d.Receipt?.Supplier != null))
            result[d.StockMovementId!.Value] = (d.Receipt!.Supplier!.ComercialName, d.Receipt.Number);

        var deliveryDetailIds = movements
            .Where(m => m.Entity == StockMovementEntities.DeliveryNote && m.EntityId.HasValue)
            .Select(m => m.EntityId!.Value)
            .ToList();

        if (deliveryDetailIds.Count > 0)
        {
            var deliveryDetails = await unitOfWork.DeliveryNotes.Details.FindAsyncWithQueryParams(
                d => deliveryDetailIds.Contains(d.Id),
                q => q.Include(d => d.DeliveryNote).ThenInclude(dn => dn!.Customer));

            var deliveryDetailsById = deliveryDetails.ToDictionary(d => d.Id);

            foreach (var m in movements.Where(m => m.Entity == StockMovementEntities.DeliveryNote && m.EntityId.HasValue))
            {
                if (deliveryDetailsById.TryGetValue(m.EntityId!.Value, out var detail) && detail.DeliveryNote?.Customer != null)
                    result[m.Id] = (detail.DeliveryNote.Customer.ComercialName, detail.DeliveryNote.Number);
            }
        }

        return result;
    }

    private static void AttachMovementsToNodes(LotTraceabilityNode node, Dictionary<Guid, List<LotStockMovementDto>> movementsByLot)
    {
        if (movementsByLot.TryGetValue(node.LotId, out var movements))
            node.Movements = movements;

        foreach (var child in node.Children)
            AttachMovementsToNodes(child, movementsByLot);
    }

    // Consumption/production always qualify; INPUT/OUTPUT only qualify when not an internal WorkOrderPhase supply/return transfer
    private static bool IsRelevantForTraceability(StockMovement m) =>
        m.MovementType is StockMovementType.CONSUMPTION or StockMovementType.PRODUCTION
        || m.Entity != StockMovementEntities.WorkOrderPhase;

    private static IEnumerable<Guid> CollectAllLotIds(LotTraceabilityNode node)
    {
        yield return node.LotId;
        foreach (var child in node.Children)
        {
            foreach (var lotId in CollectAllLotIds(child))
                yield return lotId;
        }
    }
}
