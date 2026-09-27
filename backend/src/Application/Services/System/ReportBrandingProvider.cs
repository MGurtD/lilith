using Application.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services.System;

/// <summary>
/// Resolves the branding printed on PDF reports from the same Enterprise branding the UI uses.
/// Image bytes are cached by file id: every upload creates a new file, so entries never go stale.
/// </summary>
public class ReportBrandingProvider(IBrandingService brandingService, IMemoryCache cache) : IReportBrandingProvider
{
    private static readonly TimeSpan ImageCacheDuration = TimeSpan.FromHours(12);

    public async Task<ReportBranding> GetCurrent(CancellationToken cancellationToken = default)
    {
        var branding = await brandingService.GetCurrent();

        var logo = await GetImage(BrandingLogoSlot.Main, branding.MainLogoVersion, cancellationToken);
        var watermark = branding.WatermarkEnabled
            ? await GetImage(BrandingLogoSlot.Watermark, branding.WatermarkVersion, cancellationToken)
            : null;

        return new ReportBranding(
            branding.BrandName,
            BrandingPalette.NormalizeOrDefault(branding.PrimaryColor),
            logo,
            branding.WatermarkEnabled,
            watermark);
    }

    private async Task<byte[]?> GetImage(BrandingLogoSlot slot, string? fileVersion, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(fileVersion))
            return null;

        var cacheKey = $"report-branding:{slot}:{fileVersion}";
        if (cache.TryGetValue(cacheKey, out byte[]? cached))
            return cached;

        var content = await brandingService.GetCurrentLogo(slot);
        if (content is null)
            return null;

        await using var stream = content.Content;
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        cache.Set(cacheKey, bytes, ImageCacheDuration);
        return bytes;
    }
}
