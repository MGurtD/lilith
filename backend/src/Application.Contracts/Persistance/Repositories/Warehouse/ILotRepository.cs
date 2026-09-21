using Domain.Entities.Warehouse;

namespace Application.Contracts;

public interface ILotRepository : IRepository<Lot, Guid>
{
    // Flat edges (parent lot -> child lot) for the recursive lot-traceability chain, walked in memory into a tree by LotTraceabilityService.
    Task<List<LotTraceabilityEdgeRow>> GetBackwardTraceabilityEdges(Guid lotId);
    Task<List<LotTraceabilityEdgeRow>> GetForwardTraceabilityEdges(Guid lotId);
}

/// <summary>
/// Flat row produced by the WITH RECURSIVE traceability CTEs (see LotRepository). Purchase origins and
/// stock movements for each lot are attached separately by LotTraceabilityService, not carried on this row.
/// </summary>
public class LotTraceabilityEdgeRow
{
    public Guid? ParentLotId { get; set; }
    public Guid LotId { get; set; }
    public int Depth { get; set; }
    public decimal Quantity { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceDescription { get; set; } = string.Empty;
}
