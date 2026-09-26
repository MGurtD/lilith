namespace Application.Contracts.Ingestion;

public interface IInvoiceIngestionService
{
    Task<IngestPurchaseInvoiceResponse> IngestAsync(
        Stream pdfStream,
        string fileName,
        CancellationToken ct = default);
}