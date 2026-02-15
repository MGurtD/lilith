namespace Domain.Entities.Sales
{
    public class SalesInvoiceDueDate : Entity
    {
        public Guid SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }

    }
}
