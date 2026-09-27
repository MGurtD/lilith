namespace Application.Contracts;

/// <summary>
/// Branding applied to generated PDF reports. Image slots are null when the tenant has not
/// uploaded one, and the report renderer falls back to its bundled defaults.
/// </summary>
public sealed record ReportBranding(
    string BrandName,
    string Palette,
    byte[]? Logo,
    bool WatermarkEnabled,
    byte[]? Watermark)
{
    public static ReportBranding Default { get; } = new(
        BrandingResponse.Default.BrandName,
        BrandingPalette.Default,
        null,
        true,
        null);
}
