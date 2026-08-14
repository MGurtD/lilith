using Application.Contracts;
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

        // Base case: this lot has its own ReceiptDetail => purchase origin, do not descend further.
        if (row.ReceiptId.HasValue)
        {
            node.PurchaseOrigins.Add(new PurchaseOriginDto
            {
                LotId = row.LotId,
                LotCode = row.LotCode,
                ReferenceId = row.ReferenceId,
                ReferenceCode = row.ReferenceCode,
                ReferenceDescription = row.ReferenceDescription,
                Quantity = row.Quantity,
                SupplierId = row.SupplierId!.Value,
                SupplierName = row.SupplierName ?? string.Empty,
                ReceiptId = row.ReceiptId.Value,
                ReceiptNumber = row.ReceiptNumber ?? string.Empty,
                ReceiptDate = row.ReceiptDate!.Value
            });
            return node;
        }

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

    // Attaches the stock movements (provisioning, consumption, production, etc.) of every lot in the tree.
    private async Task AttachMovements(LotTraceabilityNode root)
    {
        var lotIds = CollectAllLotIds(root).Distinct().ToList();
        var movements = await unitOfWork.StockMovements.GetByLotIds(lotIds);
        var movementsByLot = movements
            .Where(m => m.LotId.HasValue)
            .GroupBy(m => m.LotId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(m => new LotStockMovementDto
                {
                    MovementId = m.Id,
                    MovementType = m.MovementType,
                    Quantity = m.Quantity,
                    MovementDate = m.MovementDate,
                    Description = m.Description,
                    LocationId = m.LocationId,
                    LocationName = m.Location?.Name ?? string.Empty,
                    Entity = m.Entity,
                    EntityId = m.EntityId
                }).ToList());

        AttachMovementsToNodes(root, movementsByLot);
    }

    private static void AttachMovementsToNodes(LotTraceabilityNode node, Dictionary<Guid, List<LotStockMovementDto>> movementsByLot)
    {
        if (movementsByLot.TryGetValue(node.LotId, out var movements))
            node.Movements = movements;

        foreach (var child in node.Children)
            AttachMovementsToNodes(child, movementsByLot);
    }

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
