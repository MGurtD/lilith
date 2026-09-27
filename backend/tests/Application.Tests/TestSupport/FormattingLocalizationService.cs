using Application.Contracts;

namespace Application.Tests.TestSupport;

/// <summary>
/// Localization double that returns "Key|arg1|arg2", so tests can assert both the
/// resource key and the arguments passed to it without a translation dictionary.
/// </summary>
public sealed class FormattingLocalizationService : ILocalizationService
{
    public string GetLocalizedString(string key, params object[] arguments) =>
        string.Join('|', new object[] { key }.Concat(arguments));

    public string GetLocalizedStringForCulture(string key, string culture, params object[] arguments) =>
        GetLocalizedString(key, arguments);

    public Dictionary<string, string> GetAllTranslations() => [];
    public Dictionary<string, string> GetAllTranslationsForCulture(string culture) => [];
    public string[] GetSupportedCultures() => ["ca", "es", "en"];
}
