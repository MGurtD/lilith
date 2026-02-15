using Domain.Entities.Sales;

namespace Application.Contracts
{
    public interface ISalesInvoiceRepository : IRepository<SalesInvoice, Guid>
    {
        IRepository<SalesInvoiceDetail, Guid> InvoiceDetails { get; }
        IRepository<SalesInvoiceImport, Guid> InvoiceImports { get; }
        IRepository<SalesInvoiceDueDate, Guid> InvoiceDueDates { get; }
        Task<SalesInvoice?> GetHeader(Guid id);
        Task<IEnumerable<SalesInvoice>> GetIntegrationsBetweenDates(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<SalesInvoice>> GetPendingToIntegrate(DateTime? toDate, Guid? initialStatusId);

    }
}

