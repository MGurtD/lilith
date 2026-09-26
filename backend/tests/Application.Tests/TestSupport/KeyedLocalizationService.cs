using Application.Contracts;

namespace Application.Tests.TestSupport;

/// <summary>
/// Localization double that resolves keys from an explicit dictionary and
/// tracks how many lookups were performed.  Use this when tests assert on
/// the exact user-facing message text or on the number of lookups triggered.
/// </summary>
public sealed class KeyedLocalizationService(IReadOnlyDictionary<string, string> keys) : ILocalizationService
{
    /// <summary>Number of <see cref="GetLocalizedString"/> calls made so far.</summary>
    public int LookupCount { get; private set; }

    public string GetLocalizedString(string key, params object[] arguments)
    {
        LookupCount++;
        return keys.TryGetValue(key, out var template)
            ? string.Format(template, arguments)
            : key;
    }

    public string GetLocalizedStringForCulture(string key, string culture, params object[] arguments) =>
        GetLocalizedString(key, arguments);

    public Dictionary<string, string> GetAllTranslations() => new(keys);
    public Dictionary<string, string> GetAllTranslationsForCulture(string culture) => new(keys);
    public string[] GetSupportedCultures() => ["ca", "es", "en"];
}
