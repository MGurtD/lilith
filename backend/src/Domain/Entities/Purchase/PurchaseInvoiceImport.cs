namespace Domain.Entities.Purchase
{
    public class PurchaseInvoiceImport : Entity
    {
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }

        public Guid PurchaseInvoiceId { get; set; }
        public PurchaseInvoice? PurchaseInvoice { get; set; }

        public Guid TaxId { get; set; }
        public Tax? Tax { get; set; }
    }
}
