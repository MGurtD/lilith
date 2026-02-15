using Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalizationController(ILocalizationService localizationService) : ControllerBase
    {
        /// <summary>
        /// Get all translations for the current request culture (auto-detected or default)
        /// </summary>
        [HttpGet("all")]
        public IActionResult GetAllTranslations()
        {
            try
            {
                var currentCulture = CultureInfo.CurrentUICulture.Name;
                var translations = localizationService.GetAllTranslations();
                var supportedCultures = localizationService.GetSupportedCultures();

                return Ok(new
                {
                    Culture = currentCulture,
                    SupportedCultures = supportedCultures,
                    TranslationCount = translations.Count,
                    Translations = translations
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = "Failed to retrieve translations",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all translations for a specific culture
        /// </summary>
        [HttpGet("all/{culture}")]
        public IActionResult GetAllTranslationsForCulture(string culture)
        {
            try
            {
                var supportedCultures = localizationService.GetSupportedCultures();
                
                if (!supportedCultures.Contains(culture.ToLowerInvariant()))
                {
                    return BadRequest(new
                    {
                        Error = $"Unsupported culture: {culture}",
                        SupportedCultures = supportedCultures
                    });
                }

                var translations = localizationService.GetAllTranslationsForCulture(culture);

                return Ok(new
                {
                    Culture = culture,
                    SupportedCultures = supportedCultures,
                    TranslationCount = translations.Count,
                    Translations = translations
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = $"Failed to retrieve translations for culture: {culture}",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Compare translations across all supported cultures
        /// </summary>
        [HttpGet("compare")]
        public IActionResult CompareAllCultures()
        {
            try
            {
                var supportedCultures = localizationService.GetSupportedCultures();
                var comparison = new Dictionary<string, Dictionary<string, string>>();

                foreach (var culture in supportedCultures)
                {
                    comparison[culture] = localizationService.GetAllTranslationsForCulture(culture);
                }

                // Get all unique keys across all cultures
                var allKeys = comparison.Values
                    .SelectMany(dict => dict.Keys)
                    .Distinct()
                    .OrderBy(key => key)
                    .ToArray();

                // Create a comparison matrix
                var comparisonMatrix = allKeys.ToDictionary(key => key, key =>
                    supportedCultures.ToDictionary(culture => culture, culture =>
                        comparison[culture].TryGetValue(key, out var value) ? value : "[MISSING]"
                    )
                );

                return Ok(new
                {
                    SupportedCultures = supportedCultures,
                    TotalKeys = allKeys.Length,
                    Comparison = comparisonMatrix
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = "Failed to compare translations",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get list of supported cultures
        /// </summary>
        [HttpGet("cultures")]
        public IActionResult GetSupportedCultures()
        {
            var supportedCultures = localizationService.GetSupportedCultures();
            
            return Ok(new
            {
                SupportedCultures = supportedCultures,
                DefaultCulture = "ca",
                CurrentCulture = CultureInfo.CurrentUICulture.Name
            });
        }
    }
}