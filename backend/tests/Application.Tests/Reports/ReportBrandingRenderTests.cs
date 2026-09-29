using Application.Contracts;
using Application.Tests.TestSupport;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Infrastructure.Reports;
using Infrastructure.Reports.Common;
using NSubstitute;
using QuestPDF.Infrastructure;
using Xunit;

namespace Application.Tests.Reports;

/// <summary>
/// Palette mapping, style fallbacks and render smoke tests for branded PDF reports.
/// </summary>
public class ReportBrandingRenderTests
{
    // Smallest valid PNG: 1x1 transparent pixel.
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    public ReportBrandingRenderTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Fact]
    public void Every_branding_palette_has_report_shades()
    {
        Assert.Equal(BrandingPalette.All.OrderBy(key => key), ReportPalette.Keys.OrderBy(key => key));
    }

    [Fact]
    public void Style_uses_palette_shades_shared_with_the_ui()
    {
        var style = ReportStyle.From(ReportBranding.Default with { Palette = BrandingPalette.Emerald });

        Assert.Equal("#047857", style.Primary);
        Assert.Equal("#D1FAE5", style.AccentFill);
        Assert.Equal("#ECFDF5", style.TableHeaderFill);
    }

    [Fact]
    public void Style_falls_back_to_bundled_images_when_nothing_is_uploaded()
    {
        var style = ReportStyle.From(ReportBranding.Default);

        Assert.Equal(ReportAssets.Logo, style.Logo);
        Assert.Equal(ReportAssets.Watermark, style.Watermark);
    }

    [Fact]
    public void Style_uses_uploaded_images()
    {
        var style = ReportStyle.From(ReportBranding.Default with { Logo = TinyPng, Watermark = TinyPng });

        Assert.Equal(TinyPng, style.Logo);
        Assert.Equal(TinyPng, style.Watermark);
    }

    [Fact]
    public void Style_has_no_watermark_when_it_is_disabled()
    {
        var style = ReportStyle.From(ReportBranding.Default with { WatermarkEnabled = false, Watermark = TinyPng });

        Assert.Null(style.Watermark);
    }

    [Fact]
    public void Style_ignores_images_that_cannot_be_decoded()
    {
        byte[] corrupted = [137, 80, 78, 71, 13, 10, 26, 10, 0, 0];

        var style = ReportStyle.From(ReportBranding.Default with { Logo = corrupted, Watermark = corrupted });

        Assert.Equal(ReportAssets.Logo, style.Logo);
        Assert.Equal(ReportAssets.Watermark, style.Watermark);
    }

    [Fact]
    public void Style_metadata_uses_brand_name_and_product()
    {
        var metadata = ReportStyle.From(ReportBranding.Default with { BrandName = "  Acme  " }).Metadata("Budget 1");

        Assert.Equal("Acme", metadata.Author);
        Assert.Equal(ReportStyle.Product, metadata.Creator);
        Assert.Equal(ReportStyle.Product, metadata.Producer);
    }

    [Theory]
    [MemberData(nameof(Brandings))]
    public async Task Budget_pdf_renders_with_each_branding(ReportBranding branding)
    {
        var sut = new BudgetPdfService(Provider(branding));

        var pdf = await sut.Generate(BudgetReport());

        AssertPdf(pdf);
    }

    [Theory]
    [MemberData(nameof(Brandings))]
    public async Task Work_order_pdf_renders_with_each_branding(ReportBranding branding)
    {
        var sut = new WorkOrderPdfService(NullLocalizationService.Instance, Provider(branding));

        var pdf = await sut.Generate(WorkOrderReport());

        AssertPdf(pdf);
    }

    public static TheoryData<ReportBranding> Brandings() =>
    [
        ReportBranding.Default,
        ReportBranding.Default with { Palette = BrandingPalette.Rose, Logo = TinyPng, Watermark = TinyPng },
        ReportBranding.Default with { Palette = BrandingPalette.Black, WatermarkEnabled = false },
    ];

    // -------- helpers --------

    private static IReportBrandingProvider Provider(ReportBranding branding)
    {
        var provider = Substitute.For<IReportBrandingProvider>();
        provider.GetCurrent(Arg.Any<CancellationToken>()).Returns(branding);
        return provider;
    }

    private static void AssertPdf(byte[] pdf)
    {
        Assert.True(pdf.Length > 1000);
        Assert.Equal("%PDF"u8.ToArray(), pdf[..4]);
    }

    private static BudgetReportResponse BudgetReport()
    {
        var budget = new Budget { Number = "PRE-001", Date = new DateTime(2026, 9, 1), DeliveryDays = 10 };
        budget.Details.Add(new BudgetDetail { Quantity = 5, Description = "Shaft", UnitPrice = 12.5m, Amount = 62.5m });

        // The localization stub returns resource keys, which are longer than the real labels the fixed-height header is sized for.
        return new BudgetReportResponse("ca", NullLocalizationService.Instance)
        {
            Title = "Pressupost",
            HeaderNumber = "Número",
            HeaderDate = "Data",
            Budget = budget,
            Customer = new Customer { TaxName = "Customer SL", VatNumber = "B12345678" },
            Site = new Site { Address = "Carrer Major 1", City = "Girona", PostalCode = "17001", VatNumber = "B87654321" },
            Enterprise = new Enterprise { Name = "Acme" },
            Total = 62.5m
        };
    }

    private static WorkOrderReportResponse WorkOrderReport() => new("ca")
    {
        Site = new Site { VatNumber = "B87654321" },
        Enterprise = new Enterprise { Name = "Acme" },
        Order = new WorkOrderReportDto
        {
            Code = "OF-001",
            ReferenceCode = "REF-1",
            ReferenceDescription = "Shaft",
            PlannedDate = new DateTime(2026, 9, 1),
            PlannedQuantity = 10,
            StatusName = "Planned",
            HasExternalWork = false,
            Comment = string.Empty
        },
        Phases = [],
        BillOfMaterials = []
    };
}
