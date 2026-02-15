namespace Domain.Entities.Sales
{
    public class SalesInvoiceDetail : Entity
    {
        public Guid SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        public Guid? DeliveryNoteDetailId { get; set; }
        public DeliveryNoteDetail? DeliveryNoteDetail { get; set; }

        public Guid TaxId { get; set; }
        public Tax? Tax { get; set; }

        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; } = decimal.Zero;
        public decimal UnitPrice { get; set; } = decimal.Zero;
        public decimal TotalCost { get; set; } = decimal.Zero;
        public decimal Amount { get; set; } = decimal.Zero;

        public void SetDeliveryNoteDetail(DeliveryNoteDetail deliveryNoteDetail)
        {
            DeliveryNoteDetailId = deliveryNoteDetail.Id;
            Description = deliveryNoteDetail.Description;
            Quantity = deliveryNoteDetail.Quantity;
            UnitCost = deliveryNoteDetail.UnitCost;
            UnitPrice = deliveryNoteDetail.UnitPrice;
            TotalCost = deliveryNoteDetail.TotalCost;
            Amount = deliveryNoteDetail.Amount;

            if (deliveryNoteDetail.Reference != null && deliveryNoteDetail.Reference.TaxId.HasValue)
                TaxId = deliveryNoteDetail.Reference!.TaxId!.Value;
        }

    }
}
