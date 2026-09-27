namespace Application.Contracts.Ingestion;

/// <summary>
/// Reads a supplier invoice document with an external extraction provider and
/// returns its raw, provider-neutral content. Implementations live in Infrastructure.
/// </summary>
public interface IInvoiceExtractor
{
    Task<ExtractedInvoice> ExtractAsync(Stream pdfStream, string fileName, CancellationToken ct = default);

    /// <summary>
    /// Whether the provider is configured and accepts the configured credentials. Drives the
    /// invoice import feature flag; implementations may cache the answer.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}
