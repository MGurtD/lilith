using Application.Utils;
using Xunit;

namespace Application.Tests.Utils;

/// <summary>
/// Unit tests for <see cref="BrandingValidator"/> — issue #64.
/// Covers PrimaryColor hex validation and LogoUrl extension validation.
/// All length constants are also asserted to stay in sync with the EF builder.
/// </summary>
public class BrandingValidatorTests
{
    // -------- PrimaryColor --------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValidPrimaryColor_null_or_empty_is_valid(string? color)
    {
        // Branding is optional per Enterprise — empty is the default state.
        Assert.True(BrandingValidator.IsValidPrimaryColor(color));
    }

    [Theory]
    [InlineData("#1e40af")]   // hex 6 lowercase
    [InlineData("#FFFFFF")]   // hex 6 uppercase
    [InlineData("#000000")]   // black
    [InlineData("#abcdef")]   // hex 6 mixed case
    public void IsValidPrimaryColor_valid_hex6_is_valid(string color)
    {
        Assert.True(BrandingValidator.IsValidPrimaryColor(color));
    }

    [Theory]
    [InlineData("#1e40af80")] // hex 8 with alpha
    [InlineData("#FFFFFFFF")] // hex 8 uppercase
    public void IsValidPrimaryColor_valid_hex8_is_valid(string color)
    {
        Assert.True(BrandingValidator.IsValidPrimaryColor(color));
    }

    [Theory]
    [InlineData("1e40af")]    // missing #
    [InlineData("#1e40")]     // too short (hex3 not supported)
    [InlineData("#1e40af00ff")] // too long (hex10)
    [InlineData("#zzzzzz")]   // invalid hex chars
    [InlineData("#GGGGGG")]   // G not a hex digit
    [InlineData("rgb(0,0,0)")] // CSS function syntax not supported
    [InlineData("#1e40af ")]  // trailing space
    public void IsValidPrimaryColor_invalid_input_returns_false(string color)
    {
        Assert.False(BrandingValidator.IsValidPrimaryColor(color));
    }

    // -------- LogoUrl --------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValidLogoUrl_null_or_empty_is_valid(string? url)
    {
        Assert.True(BrandingValidator.IsValidLogoUrl(url));
    }

    [Theory]
    [InlineData("/assets/branding/zenarchy-main.png")]
    [InlineData("logo.svg")]
    [InlineData("https://cdn.example.com/logo.JPG")]   // uppercase extension
    [InlineData("https://cdn.example.com/logo.JPEG")]
    [InlineData("/path/to/image.Png")]
    public void IsValidLogoUrl_supported_extension_is_valid(string url)
    {
        Assert.True(BrandingValidator.IsValidLogoUrl(url));
    }

    [Theory]
    [InlineData("/assets/logo.exe")]
    [InlineData("https://example.com/file.pdf")]
    [InlineData("/path/to/image.bmp")]
    [InlineData("logo")]           // no extension
    [InlineData("logo.png.exe")]   // double extension, last is not allowed
    public void IsValidLogoUrl_unsupported_extension_returns_false(string url)
    {
        Assert.False(BrandingValidator.IsValidLogoUrl(url));
    }

    // -------- Length constants stay in sync with EF builder --------

    [Fact]
    public void Length_constants_match_EnterpriseBuilder()
    {
        // If these change, the EnterpriseBuilder configuration must change too
        // (see EnterpriseBuilder.cs in Infrastructure).
        Assert.Equal(50, BrandingValidator.MaxLengthTheme);
        Assert.Equal(9, BrandingValidator.MaxLengthPrimaryColor);
        Assert.Equal(500, BrandingValidator.MaxLengthLogoUrl);
        Assert.Equal(60, BrandingValidator.MaxLengthTitleSidebar);
    }
}