namespace Domain.Entities.Purchase
{
    public class Receipt : Entity
    {
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Guid SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public Guid ExerciseId { get; set; }
        public Exercise? Exercise { get; set; }
        public Guid StatusId { get; set; }
        public Status? Status { get; set; }
        public string SupplierNumber { get; set; } = string.Empty;

        public Guid? PurchaseInvoiceId { get; set; }
        public PurchaseInvoice? PurchaseInvoice { get; set; }

        public ICollection<ReceiptDetail> Details { get; set; } = [];
    }
}
