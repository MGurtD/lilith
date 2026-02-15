using Application.Contracts;

namespace Application.Contracts;

public interface ISalesInvoiceReportService
{
    Task<InvoiceReportDto?> GetReportById(Guid id);
}
