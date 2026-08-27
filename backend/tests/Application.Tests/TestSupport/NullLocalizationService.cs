using Application.Contracts;

namespace Application.Tests.TestSupport;

/// <summary>
/// Localization double that returns the key unchanged.
/// Use this when tests do not assert on message content — they only verify
/// which key was requested (or simply that the call succeeds/fails).
/// </summary>
public sealed class NullLocalizationService : ILocalizationService
{
    public static NullLocalizationService Instance { get; } = new();

    public string GetLocalizedString(string key, params object[] arguments) => key;
    public string GetLocalizedStringForCulture(string key, string culture, params object[] arguments) => key;
    public Dictionary<string, string> GetAllTranslations() => [];
    public Dictionary<string, string> GetAllTranslationsForCulture(string culture) => [];
    public string[] GetSupportedCultures() => ["ca", "es", "en"];
}
