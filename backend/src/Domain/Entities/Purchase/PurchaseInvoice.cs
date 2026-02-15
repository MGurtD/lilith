using Domain.Entities.Shared;

namespace Domain.Entities.Purchase
{
    public class PurchaseInvoice : Entity
    {
        public string Number { get; set; } = string.Empty;
        public DateTime PurchaseInvoiceDate { get; set; }

        public decimal BaseAmount { get; set; }
        public decimal TransportAmount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public int ExtraTaxPercentatge { get; set; }
        public decimal ExtraTaxAmount { get; set; }

        public string SupplierNumber { get; set; } = string.Empty;
        public Supplier? Supplier { get; set; }
        public Guid SupplierId { get; set; }

        public Exercise? Exercice { get; set; }
        public Guid? ExerciceId { get; set; }

        public InvoiceSerie? PurchaseInvoiceSerie { get; set; }
        public Guid? PurchaseInvoiceSerieId { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }
        public Guid PaymentMethodId { get; set; }

        public Status? Status { get; set; }
        public Guid? StatusId { get; set; }

        public ICollection<PurchaseInvoiceDueDate>? PurchaseInvoiceDueDates { get; set; }
        public ICollection<PurchaseInvoiceImport>? PurchaseInvoiceImports { get; set; }
    }

}
