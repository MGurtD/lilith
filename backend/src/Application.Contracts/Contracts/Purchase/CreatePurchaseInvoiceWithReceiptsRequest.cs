using System.ComponentModel.DataAnnotations;
using Domain.Entities.Purchase;

namespace Application.Contracts
{
    /// <summary>
    /// Creates a purchase invoice and links the given uninvoiced receipts of its supplier
    /// in a single transaction.
    /// </summary>
    public class CreatePurchaseInvoiceWithReceiptsRequest
    {
        [Required]
        public PurchaseInvoice Invoice { get; set; } = new();
        public List<Guid> ReceiptIds { get; set; } = [];
    }
}
