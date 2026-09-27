namespace Application.Contracts;

public sealed record BrandingResponse(
    string BrandName,
    string? PrimaryColor,
    bool HasMainLogo,
    bool HasSidebarLogo,
    string Version,
    string? MainLogoVersion,
    string? SidebarLogoVersion,
    bool HasWatermark,
    string? WatermarkVersion,
    bool WatermarkEnabled)
{
    public static BrandingResponse Default { get; } = new(
        "Temges",
        BrandingPalette.Default,
        false,
        false,
        "default",
        null,
        null,
        false,
        null,
        true);
}

public enum BrandingLogoSlot
{
    Main,
    Sidebar,
    Watermark
}

public sealed record BrandingLogoContent(
    Stream Content,
    string ContentType,
    DateTime LastModified);
