using Api.Models;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text.Json;

namespace Application.Services.System
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly Dictionary<string, Dictionary<string, string>> _localizations = new();
        private readonly string _resourcesPath;
        private readonly string _baseName;
        private readonly ILogger<JsonStringLocalizer> _logger;

        public JsonStringLocalizer(string resourcesPath, string baseName, ILogger<JsonStringLocalizer> logger)
        {
            _resourcesPath = resourcesPath;
            _baseName = baseName;
            _logger = logger;
            LoadLocalizations();
        }

        public LocalizedString this[string name] => GetLocalizedString(name);

        public LocalizedString this[string name, params object[] arguments] => GetLocalizedString(name, arguments);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = CultureInfo.CurrentUICulture.Name;

            if (_localizations.TryGetValue(culture, out var strings))
            {
                return strings.Select(s => new LocalizedString(s.Key, s.Value, false));
            }

            return [];
        }

        private LocalizedString GetLocalizedString(string name, params object[] arguments)
        {
            var culture = CultureInfo.CurrentUICulture.Name;

            // Try exact culture match first (e.g., "ca-ES")
            if (_localizations.TryGetValue(culture, out var cultureStrings))
            {
                if (cultureStrings.TryGetValue(name, out var value))
                {
                    return new LocalizedString(name, FormatString(value, arguments), false);
                }
            }

            // Try language-only match (e.g., "ca" for "ca-ES")
            var languageCode = culture.Split('-')[0];
            if (_localizations.TryGetValue(languageCode, out var languageStrings))
            {
                if (languageStrings.TryGetValue(name, out var value))
                {
                    return new LocalizedString(name, FormatString(value, arguments), false);
                }
            }

            // Fall back to English if available
            if (_localizations.TryGetValue("en", out var englishStrings))
            {
                if (englishStrings.TryGetValue(name, out var value))
                {
                    return new LocalizedString(name, FormatString(value, arguments), false);
                }
            }

            // Return the key itself if not found
            return new LocalizedString(name, name, true);
        }

        private static string FormatString(string template, params object[] arguments)
        {
            if (arguments?.Length > 0)
            {
                try
                {
                    return string.Format(template, arguments);
                }
                catch
                {
                    return template;
                }
            }
            return template;
        }

        private void LoadLocalizations()
        {
            // The baseName is "LocalizationService", so we look for files in Resources/LocalizationService/
            var resourcePath = Path.Combine(_resourcesPath, _baseName);

            if (!Directory.Exists(resourcePath))
            {
                // Log that the directory doesn't exist for debugging
                _logger.LogWarning($"Resource directory not found: {Path.GetFullPath(resourcePath)}");
                return;
            }

            var jsonFiles = Directory.GetFiles(resourcePath, "*.json");
            _logger.LogInformation($"Found {jsonFiles.Length} localization files in {resourcePath}");

            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var localizationFile = JsonSerializer.Deserialize<LocalizationFile>(json);

                    if (localizationFile != null && !string.IsNullOrEmpty(localizationFile.Culture) && localizationFile.Texts != null)
                    {
                        _localizations[localizationFile.Culture] = localizationFile.Texts;
                        _logger.LogInformation($"Loaded {localizationFile.Texts.Count} translations for culture '{localizationFile.Culture}'");
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid localization file format: {file}. Expected format with 'culture' and 'texts' properties.");
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue loading other files
                    _logger.LogError(ex, $"Error loading localization file {file}");
                }
            }
        }
    }
}




