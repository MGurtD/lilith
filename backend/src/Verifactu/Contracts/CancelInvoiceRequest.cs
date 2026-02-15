using Domain.Entities.Production;
using Domain.Entities.Sales;

namespace Verifactu.Contracts;

public class CancelInvoiceRequest
{
    public required SalesInvoice InvoiceToCancel { get; set; }
    public required Enterprise Enterprise { get; set; }
    public SalesInvoice? PreviousNotificatedInvoice { get; set; } = null;
    public string? PreviousHash { get; set; } = null;
}
