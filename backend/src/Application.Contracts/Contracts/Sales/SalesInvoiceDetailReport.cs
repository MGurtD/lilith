using Domain.Entities.Sales;

namespace Application.Contracts
{
    public class SalesInvoiceDetailReport : Contract
    {
        public Guid SalesInvoiceId { get; set; }
        public int SalesOrderNumber { get; set; }
        public DateTime? SalesOrderDate { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; } = null!;
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }

    public class SalesInvoiceReport : SalesInvoice 
    { 
        IList<SalesInvoiceDetailReport> Details { get; set; }

        public SalesInvoiceReport()
        {
            Details = new List<SalesInvoiceDetailReport>();
        }
    }

}
