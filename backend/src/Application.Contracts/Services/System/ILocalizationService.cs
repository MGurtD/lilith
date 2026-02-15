namespace Application.Contracts
{
    public interface ILocalizationService
    {
        string GetLocalizedString(string key, params object[] arguments);
        string GetLocalizedStringForCulture(string key, string culture, params object[] arguments);
        Dictionary<string, string> GetAllTranslations();
        Dictionary<string, string> GetAllTranslationsForCulture(string culture);
        string[] GetSupportedCultures();
    }
}
