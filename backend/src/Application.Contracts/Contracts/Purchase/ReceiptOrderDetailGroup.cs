using Domain.Entities.Shared;

namespace Application.Contracts;

public class ReceiptOrderDetailGroup
{
    public required Reference Reference { get; set; }
    public required string Description { get; set; }
    public required int Quantity { get; set; }
    public required int ReceivedQuantity { get; set; }
    public required decimal Price { get; set; }
    public List<ReceiptOrderDetail> Details { get; set; } = [];
}

public class ReceiptOrderDetail
{
    public required Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public required DateTime? ExpectedReceiptDate { get; set; }
    public required string WorkOrder { get; set; }
    public required string WorkOrderPhase { get; set; }
    public required int Quantity { get; set; }
    public required int ReceivedQuantity { get; set; }
    public decimal PendingQuantity => Quantity - ReceivedQuantity;
}
