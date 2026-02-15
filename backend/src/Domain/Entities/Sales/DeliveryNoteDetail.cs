using Domain.Entities.Shared;

namespace Domain.Entities.Sales
{
    public class DeliveryNoteDetail : Entity
    {
        public DeliveryNote? DeliveryNote { get; set; }
        public Guid DeliveryNoteId { get; set; }
        public SalesOrderDetail? SalesOrderDetail { get; set; }
        public Guid? SalesOrderDetailId { get; set; }

        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; } = decimal.Zero;
        public decimal UnitPrice { get; set; } = decimal.Zero;
        public decimal TotalCost { get; set; } = decimal.Zero;
        public decimal Amount { get; set; } = decimal.Zero;
        public bool IsInvoiced { get; set; }


        public void SetFromOrderDetail(SalesOrderDetail detail)
        {
            SalesOrderDetailId = detail.Id;
            ReferenceId = detail.ReferenceId;
            Description = detail.Description;
            Quantity = detail.Quantity;
            UnitCost = detail.UnitCost;
            UnitPrice = detail.UnitPrice;
            TotalCost = detail.TotalCost;
            Amount = detail.Amount;
        }
    }
}
