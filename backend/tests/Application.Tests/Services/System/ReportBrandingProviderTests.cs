using Application.Contracts;
using Application.Services.System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Application.Tests.Services.System;

/// <summary>
/// Unit tests for <see cref="ReportBrandingProvider"/>.
/// </summary>
public class ReportBrandingProviderTests
{
    private static readonly byte[] LogoBytes = [1, 2, 3];
    private static readonly byte[] WatermarkBytes = [4, 5, 6];

    [Fact]
    public async Task GetCurrent_uses_uploaded_images_and_palette_when_watermark_is_enabled()
    {
        var branding = BrandingWith(mainLogoVersion: "logo-v1", watermarkVersion: "wm-v1", watermarkEnabled: true);
        var sut = BuildSut(branding, out _);

        var result = await sut.GetCurrent();

        Assert.Equal("Acme", result.BrandName);
        Assert.Equal(BrandingPalette.Teal, result.Palette);
        Assert.Equal(LogoBytes, result.Logo);
        Assert.True(result.WatermarkEnabled);
        Assert.Equal(WatermarkBytes, result.Watermark);
    }

    [Fact]
    public async Task GetCurrent_leaves_images_null_when_nothing_is_uploaded()
    {
        var sut = BuildSut(BrandingWith(mainLogoVersion: null, watermarkVersion: null, watermarkEnabled: true), out var brandingService);

        var result = await sut.GetCurrent();

        Assert.Null(result.Logo);
        Assert.True(result.WatermarkEnabled);
        Assert.Null(result.Watermark);
        await brandingService.DidNotReceive().GetCurrentLogo(Arg.Any<BrandingLogoSlot>());
    }

    [Fact]
    public async Task GetCurrent_skips_the_watermark_when_it_is_disabled()
    {
        var sut = BuildSut(BrandingWith(mainLogoVersion: null, watermarkVersion: "wm-v1", watermarkEnabled: false), out var brandingService);

        var result = await sut.GetCurrent();

        Assert.False(result.WatermarkEnabled);
        Assert.Null(result.Watermark);
        await brandingService.DidNotReceive().GetCurrentLogo(BrandingLogoSlot.Watermark);
    }

    [Fact]
    public async Task GetCurrent_reads_each_image_file_once_per_version()
    {
        var sut = BuildSut(BrandingWith(mainLogoVersion: "logo-v1", watermarkVersion: "wm-v1", watermarkEnabled: true), out var brandingService);

        await sut.GetCurrent();
        var second = await sut.GetCurrent();

        Assert.Equal(LogoBytes, second.Logo);
        Assert.Equal(WatermarkBytes, second.Watermark);
        await brandingService.Received(1).GetCurrentLogo(BrandingLogoSlot.Main);
        await brandingService.Received(1).GetCurrentLogo(BrandingLogoSlot.Watermark);
    }

    [Fact]
    public async Task GetCurrent_falls_back_to_the_default_when_an_image_file_cannot_be_read()
    {
        var sut = BuildSut(BrandingWith(mainLogoVersion: "logo-v1", watermarkVersion: "wm-v1", watermarkEnabled: true), out var brandingService);
        brandingService.GetCurrentLogo(BrandingLogoSlot.Main)
            .Returns<Task<BrandingLogoContent?>>(_ => throw new UnauthorizedAccessException("denied"));

        var result = await sut.GetCurrent();

        Assert.Null(result.Logo);
        Assert.Equal(WatermarkBytes, result.Watermark);
    }

    // -------- helpers --------

    private static ReportBrandingProvider BuildSut(BrandingResponse branding, out IBrandingService brandingService)
    {
        brandingService = Substitute.For<IBrandingService>();
        brandingService.GetCurrent().Returns(branding);
        brandingService.GetCurrentLogo(BrandingLogoSlot.Main).Returns(_ => Content(LogoBytes));
        brandingService.GetCurrentLogo(BrandingLogoSlot.Watermark).Returns(_ => Content(WatermarkBytes));

        return new ReportBrandingProvider(
            brandingService,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<ReportBrandingProvider>.Instance);
    }

    private static BrandingLogoContent Content(byte[] bytes) =>
        new(new MemoryStream(bytes), "image/png", DateTime.UtcNow);

    private static BrandingResponse BrandingWith(string? mainLogoVersion, string? watermarkVersion, bool watermarkEnabled) =>
        new(
            "Acme",
            BrandingPalette.Teal,
            mainLogoVersion is not null,
            false,
            "v1",
            mainLogoVersion,
            null,
            watermarkVersion is not null,
            watermarkVersion,
            watermarkEnabled);
}
