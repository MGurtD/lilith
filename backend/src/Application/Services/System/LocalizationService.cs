using Application.Contracts;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Application.Services.System
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizerFactory _localizerFactory;
        private readonly IStringLocalizer _localizer;
        private readonly ILanguageCatalog _languageCatalog;

        public LocalizationService(IStringLocalizerFactory localizerFactory, ILanguageCatalog languageCatalog)
        {
            _localizerFactory = localizerFactory;
            _languageCatalog = languageCatalog;
            _localizer = _localizerFactory.Create("LocalizationService", string.Empty);
        }

        public string GetLocalizedString(string key, params object[] arguments)
        {
            var localizedString = _localizer[key, arguments];
            return localizedString.ResourceNotFound ? key : localizedString.Value;
        }

        public string GetLocalizedStringForCulture(string key, string culture, params object[] arguments)
        {
            var currentCulture = CultureInfo.CurrentCulture;
            var currentUICulture = CultureInfo.CurrentUICulture;

            try
            {
                var cultureInfo = new CultureInfo(culture);
                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;

                return GetLocalizedString(key, arguments);
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
                CultureInfo.CurrentUICulture = currentUICulture;
            }
        }

        public Dictionary<string, string> GetAllTranslations()
        {
            var allStrings = _localizer.GetAllStrings(false);
            return allStrings.ToDictionary(s => s.Name, s => s.Value);
        }

        public Dictionary<string, string> GetAllTranslationsForCulture(string culture)
        {
            var currentCulture = CultureInfo.CurrentCulture;
            var currentUICulture = CultureInfo.CurrentUICulture;

            try
            {
                var cultureInfo = new CultureInfo(culture);
                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;

                return GetAllTranslations();
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
                CultureInfo.CurrentUICulture = currentUICulture;
            }
        }

        public string[] GetSupportedCultures()
        {
            var codes = _languageCatalog
                        .GetAllAsync()
                        .GetAwaiter()
                        .GetResult()
                        .Select(l => l.Code.ToLowerInvariant())
                        .Distinct()
                        .ToArray();
            return codes.Length > 0 ? codes : ["ca", "es", "en"];
        }
    }
}




