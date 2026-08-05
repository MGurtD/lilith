namespace Application.Contracts;

public sealed record BrandingResponse(
    string BrandName,
    string? PrimaryColor,
    bool HasMainLogo,
    bool HasSidebarLogo,
    string Version)
{
    public static BrandingResponse Default { get; } = new(
        "Temges",
        BrandingPalette.Default,
        false,
        false,
        "default");
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
