namespace Application.Contracts.Ingestion;

public interface IInvoiceIngestionService
{
    Task<IngestPurchaseInvoiceResponse> IngestAsync(
        Stream pdfStream,
        string fileName,
        CancellationToken ct = default);

    /// <summary>Feature flag: invoice import is offered only when the provider is usable.</summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}