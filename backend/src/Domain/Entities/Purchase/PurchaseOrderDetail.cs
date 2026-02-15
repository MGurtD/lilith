using Domain.Entities.Production;
using Domain.Entities.Shared;

namespace Domain.Entities.Purchase
{
    public class PurchaseOrderDetail : Entity
    {
        public Guid PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid? WorkOrderPhaseId { get; set; }
        public WorkOrderPhase? WorkOrderPhase { get; set; }
        public Guid StatusId { get; set; }
        public Status? Status { get; set; }

        public DateTime? ExpectedReceiptDate { get; set; }
        public int Quantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; } = decimal.Zero;
        public decimal Amount { get; set; } = decimal.Zero;
    }
}
