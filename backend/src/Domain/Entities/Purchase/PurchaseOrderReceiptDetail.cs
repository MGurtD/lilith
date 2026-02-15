namespace Domain.Entities.Purchase;

public class PurchaseOrderReceiptDetail: Entity
{
    public Guid PurchaseOrderDetailId { get; set; }
    public PurchaseOrderDetail? PurchaseOrderDetail { get; set; }
    public Guid ReceiptDetailId { get; set; }
    public ReceiptDetail? ReceiptDetail { get; set; }

    public decimal Quantity { get; set; }
    public string User { get; set; } = string.Empty;
}