namespace Application.Contracts;

public class ConsumePhaseStockRequest : Contract
{
    public Guid WorkcenterId { get; set; }
    public Guid WorkOrderPhaseId { get; set; }
    public List<ConsumeStockEntry> Entries { get; set; } = [];
}

public class ConsumeStockEntry
{
    public Guid StockId { get; set; }
    public List<RemainingPiece> RemainingPieces { get; set; } = [];
}

public class RemainingPiece
{
    public int Quantity { get; set; }
    public decimal Width { get; set; }
    public decimal Length { get; set; }
    public decimal Height { get; set; }
    public decimal Diameter { get; set; }
    public decimal Thickness { get; set; }
}
