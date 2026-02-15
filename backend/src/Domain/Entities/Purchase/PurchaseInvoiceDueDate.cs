namespace Domain.Entities.Purchase
{
    public class PurchaseInvoiceDueDate : Entity
    {
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }

        public Guid PurchaseInvoiceId { get; set; }
        public PurchaseInvoice? PurchaseInvoice { get; set; }
    }
}
