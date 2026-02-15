namespace Application.Contracts;

/// <summary>
/// Base class for report responses that provides language-aware translations.
/// </summary>
/// <remarks>
/// Constructs a report response with the specified language code.
/// </remarks>
/// <param name="languageCode">Language/culture code to use for lookups.</param>
public abstract class ReportResponse(string languageCode)
{
    /// <summary>
    /// Two-letter language/culture code (e.g., "en").
    /// </summary>
    public string LanguageCode { get; } = string.IsNullOrWhiteSpace(languageCode) ? "ca" : languageCode;

    /// <summary>
    /// Default constructor for serialization/binding scenarios. Defaults to "ca".
    /// </summary>
    protected ReportResponse() : this("ca") { }
}
