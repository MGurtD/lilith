namespace Application.Contracts.Ingestion;

/// <summary>
/// Reads a supplier invoice document with an external extraction provider and
/// returns its raw, provider-neutral content. Implementations live in Infrastructure.
/// </summary>
public interface IInvoiceExtractor
{
    Task<ExtractedInvoice> ExtractAsync(Stream pdfStream, string fileName, CancellationToken ct = default);
}
