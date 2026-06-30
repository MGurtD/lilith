namespace Application.Ingestion;

public enum IngestionFailureKind
{
    ProviderAuthFailed,
    ProviderUnparseable,
    ProviderUnavailable,
    ProviderNotConfigured,
    UnknownTaxRate,
    SurchargeUnsupported,
}

public class IngestionException : Exception
{
    public IngestionFailureKind Kind { get; }
    public IList<decimal>? OffendingRates { get; }

    public IngestionException(IngestionFailureKind kind, string message)
        : base(message)
    {
        Kind = kind;
    }

    public IngestionException(
        IngestionFailureKind kind,
        string message,
        IList<decimal> offendingRates)
        : base(message)
    {
        Kind = kind;
        OffendingRates = offendingRates;
    }
}