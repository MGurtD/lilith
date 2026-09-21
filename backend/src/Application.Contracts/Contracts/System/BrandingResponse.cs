namespace Application.Contracts;

public sealed record BrandingResponse(
    string BrandName,
    string? PrimaryColor,
    bool HasMainLogo,
    bool HasSidebarLogo,
    string Version,
    string? MainLogoVersion,
    string? SidebarLogoVersion)
{
    public static BrandingResponse Default { get; } = new(
        "Temges",
        BrandingPalette.Default,
        false,
        false,
        "default",
        null,
        null);
}

public enum BrandingLogoSlot
{
    Main,
    Sidebar
}

public sealed record BrandingLogoContent(
    Stream Content,
    string ContentType,
    DateTime LastModified);
