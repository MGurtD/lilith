namespace Application.Contracts.Ingestion;

public enum IngestionFailureKind
{
    InvalidFile,
    NotConfigured,
    AuthFailed,
    ConfigError,
    Unparseable,
    Unavailable,
}

/// <summary>
/// Ingestion failure that prevents building a draft. The message is already localized.
/// </summary>
public class IngestionException(IngestionFailureKind kind, string message) : Exception(message)
{
    public IngestionFailureKind Kind { get; } = kind;
}
