using Application.Contracts;

namespace Application.Contracts;

public interface ISalesInvoiceReportService
{
    Task<InvoiceReportDto?> GetReportById(Guid id);
}

/// <summary>
/// Generates the QuestPDF representation of a sales invoice.
/// This contract deliberately lives outside Infrastructure so the API can use
/// it without taking a direct dependency on QuestPDF.
/// </summary>
public interface ISalesInvoicePdfService
{
    byte[] Generate(InvoiceReportDto invoice);
}
