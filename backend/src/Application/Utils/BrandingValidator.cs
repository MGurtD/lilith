using System.Text.RegularExpressions;

namespace Application.Utils
{
    /// <summary>
    /// Validates user-supplied branding values stored on <see cref="Domain.Entities.Production.Enterprise"/>.
    /// Stateless, no DI — pure functions for easy unit testing.
    /// </summary>
    public static class BrandingValidator
    {
        public const int MaxLengthTheme = 50;
        public const int MaxLengthPrimaryColor = 9;        // "#RRGGBB" (7) or "#RRGGBBAA" (9)
        public const int MaxLengthLogoUrl = 500;
        public const int MaxLengthTitleSidebar = 60;

        // "#RRGGBB" or "#RRGGBBAA"
        private static readonly Regex HexColorRegex =
            new(@"^#([0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);

        private static readonly Regex LogoExtensionRegex =
            new(@"\.(png|jpe?g|svg)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Null/empty is considered valid — branding is optional per Enterprise.
        /// </summary>
        public static bool IsValidPrimaryColor(string? color)
        {
            if (string.IsNullOrEmpty(color)) return true;
            return HexColorRegex.IsMatch(color);
        }

        /// <summary>
        /// Null/empty is valid. Non-empty must end with .png, .jpg, .jpeg or .svg.
        /// Does not check that the URL is reachable — that's a runtime concern
        /// of the frontend when it tries to load the image.
        /// </summary>
        public static bool IsValidLogoUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return LogoExtensionRegex.IsMatch(url);
        }
    }
}