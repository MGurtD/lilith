using Application.Contracts;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.Common;

/// <summary>
/// Resolved visual identity for one report render. Brand color is kept to accents
/// (title, rules, header fills); body text, amounts and tax tables stay neutral for printing.
/// </summary>
public sealed record ReportStyle(
    string BrandName,
    string Primary,
    string AccentFill,
    string TableHeaderFill,
    byte[] Logo,
    byte[]? Watermark)
{
    public const string Product = "Zenith ERP";

    public static ReportStyle Default { get; } = From(ReportBranding.Default);

    public static ReportStyle From(ReportBranding branding)
    {
        ReportFonts.EnsureRegistered();
        var shades = ReportPalette.For(branding.Palette);

        return new ReportStyle(
            string.IsNullOrWhiteSpace(branding.BrandName) ? ReportBranding.Default.BrandName : branding.BrandName.Trim(),
            shades.Shade700,
            shades.Shade100,
            shades.Shade50,
            UsableImage(branding.Logo) ?? ReportAssets.Logo,
            branding.WatermarkEnabled ? UsableImage(branding.Watermark) ?? ReportAssets.Watermark : null);
    }

    public DocumentMetadata Metadata(string title, string? subject = null) => new()
    {
        Title = title,
        Author = BrandName,
        Subject = subject ?? title,
        Creator = Product,
        Producer = Product
    };

    // An uploaded image that cannot be decoded must not break document generation.
    private static byte[]? UsableImage(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            return null;

        try
        {
            using var _ = Image.FromBinaryData(bytes);
            return bytes;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
