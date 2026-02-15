using Domain.Entities.Sales;
using System.ComponentModel.DataAnnotations;

namespace Application.Contracts
{
    public class CreateInvoiceDetailsFromOrderDetailsRequest
    {
        [Required]
        public Guid InvoiceId { get; set; }
        [Required]
        public IEnumerable<Domain.Entities.Sales.SalesOrderDetail> OrderDetails { get; set; } = Enumerable.Empty<Domain.Entities.Sales.SalesOrderDetail>();
    }
}
