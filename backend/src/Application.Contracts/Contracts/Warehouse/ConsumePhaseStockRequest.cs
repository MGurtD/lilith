namespace Application.Contracts;

public class ConsumePhaseStockRequest : Contract
{
    public Guid WorkcenterId { get; set; }
    public Guid WorkOrderPhaseId { get; set; }
    public List<ConsumeStockItem> ConsumedItems { get; set; } = [];
}

public class ConsumeStockItem
{
    public Guid StockId { get; set; }
    public int Quantity { get; set; }
    public decimal Width { get; set; }
    public decimal Length { get; set; }
    public decimal Height { get; set; }
    public decimal Diameter { get; set; }
    public decimal Thickness { get; set; }
}
