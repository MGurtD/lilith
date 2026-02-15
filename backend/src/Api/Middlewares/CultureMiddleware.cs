using System.Globalization;
using Application.Contracts;

namespace Api.Middlewares
{
    public class CultureMiddleware(RequestDelegate next, ILanguageCatalog languageCatalog)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var culture = await DetermineCultureFromRequestAsync(context, languageCatalog);
            SetCulture(culture);
            
            await next(context);
        }

        private static async Task<string> DetermineCultureFromRequestAsync(HttpContext context, ILanguageCatalog languageCatalog)
        {
            var supported = (await languageCatalog.GetAllAsync())
                            .Select(l => l.Code.ToLowerInvariant())
                            .Distinct()
                            .ToHashSet();
            if (supported.Count == 0)
            {
                supported = new[] { "ca", "es", "en" }.ToHashSet();
            }

            // 1) Query parameter override
            if (context.Request.Query.TryGetValue("culture", out var cultureQuery))
            {
                var requested = Normalize(cultureQuery.ToString());
                if (supported.Contains(requested))
                    return requested;
            }

            // 2) User claim (requires middleware to run AFTER UseAuthentication)
            var claimCulture = Normalize(context.User?.FindFirst("locale")?.Value);
            if (!string.IsNullOrWhiteSpace(claimCulture) && supported.Contains(claimCulture))
                return claimCulture;

            // 3) Accept-Language header
            var acceptLanguageHeader = context.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLanguageHeader))
            {
                foreach (var raw in acceptLanguageHeader.Split(','))
                {
                    var lang = raw.Split(';')[0].Trim().ToLowerInvariant();

                    // exact match (e.g., "es-ES")
                    var exact = Normalize(lang);
                    if (supported.Contains(exact))
                        return exact;

                    // short language part (e.g., "es")
                    var shortLang = Normalize(lang.Split('-')[0]);
                    if (supported.Contains(shortLang))
                        return shortLang;
                }
            }

            // 4) Default from catalog (or fallback)
            var defaultLang = (await languageCatalog.GetDefaultAsync())?.Code?.ToLowerInvariant();
            return !string.IsNullOrWhiteSpace(defaultLang) && supported.Contains(defaultLang)
                ? defaultLang!
                : "ca";
        }

        private static string Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();

        private static void SetCulture(string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }
    }
}