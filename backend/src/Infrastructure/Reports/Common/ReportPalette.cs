using Application.Contracts;

namespace Infrastructure.Reports.Common;

/// <summary>
/// Hex shades for each <see cref="BrandingPalette"/> key, copied from the PrimeVue Aura primitives
/// the frontend derives its primary palette from, so reports and the UI show the same color.
/// </summary>
public static class ReportPalette
{
    public sealed record Shades(string Shade50, string Shade100, string Shade200, string Shade700, string Shade800);

    private static readonly IReadOnlyDictionary<string, Shades> ByKey = new Dictionary<string, Shades>(StringComparer.Ordinal)
    {
        // The UI generates black's scale from #000000; its tints are too dark to print as fills, so neutral greys are used.
        [BrandingPalette.Black] = new("#F2F2F2", "#E4E4E7", "#D4D4D8", "#000000", "#000000"),
        [BrandingPalette.Blue] = new("#EFF6FF", "#DBEAFE", "#BFDBFE", "#1D4ED8", "#1E40AF"),
        [BrandingPalette.Indigo] = new("#EEF2FF", "#E0E7FF", "#C7D2FE", "#4338CA", "#3730A3"),
        [BrandingPalette.Emerald] = new("#ECFDF5", "#D1FAE5", "#A7F3D0", "#047857", "#065F46"),
        [BrandingPalette.Teal] = new("#F0FDFA", "#CCFBF1", "#99F6E4", "#0F766E", "#115E59"),
        [BrandingPalette.Violet] = new("#F5F3FF", "#EDE9FE", "#DDD6FE", "#6D28D9", "#5B21B6"),
        [BrandingPalette.Orange] = new("#FFF7ED", "#FFEDD5", "#FED7AA", "#C2410C", "#9A3412"),
        [BrandingPalette.Rose] = new("#FFF1F2", "#FFE4E6", "#FECDD3", "#BE123C", "#9F1239"),
    };

    public static IEnumerable<string> Keys => ByKey.Keys;

    public static Shades For(string? palette) =>
        ByKey.TryGetValue(BrandingPalette.NormalizeOrDefault(palette), out var shades)
            ? shades
            : ByKey[BrandingPalette.Default];
}
