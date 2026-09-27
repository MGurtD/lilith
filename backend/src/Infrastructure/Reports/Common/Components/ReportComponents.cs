using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.Common.Components;

public sealed record ReportParty(string Name, IReadOnlyList<string> Lines, string TaxNumber);
public sealed record ReportHeaderData(string Title, string Number, DateTime Date, string HeaderNumber, string HeaderDate, ReportParty LeftParty, ReportParty RightParty, string LanguageCode);

public sealed record ReportPageOptions(float Margin, float FontSize, bool ShowWatermark)
{
    public static ReportPageOptions Standard { get; } = new(28, 9, true);

    /// <summary>Work order sheets are denser and have never carried the watermark.</summary>
    public static ReportPageOptions Dense { get; } = new(20, 8, false);
}

/// <summary>
/// Page scaffold shared by every report: page setup, watermark, logo, header split,
/// compact header and footer with page numbers.
/// </summary>
public static class ReportPage
{
    public const string PageLabel = "Página ";
    public const string PageSeparator = " de ";
    private const float BrandRuleWidth = 1.5f;
    private const float FrameRuleWidth = 0.75f;

    public static void Configure(PageDescriptor page, ReportStyle style, ReportPageOptions options)
    {
        page.Size(PageSizes.A4);
        page.Margin(options.Margin);
        page.DefaultTextStyle(text => text.FontFamily(ReportFonts.Family).FontSize(options.FontSize));
        if (options.ShowWatermark && style.Watermark is not null)
            page.Foreground().Element(container => ReportWatermark.Compose(container, style.Watermark));
    }

    /// <summary>The first page gets the full header; the following ones get the compact header.</summary>
    public static void Header(IContainer container, Action<IContainer> firstPage, Action<IContainer> followingPages)
    {
        container.Column(column =>
        {
            column.Item().ShowOnce().Element(firstPage);
            column.Item().SkipOnce().Element(followingPages);
        });
    }

    public static void Logo(IContainer container, ReportStyle style) =>
        container.Image(style.Logo).FitArea();

    public static void BrandRule(IContainer container, ReportStyle style) =>
        container.LineHorizontal(BrandRuleWidth).LineColor(style.Primary);

    public static void CompactHeader(IContainer container, ReportStyle style, string title, string details)
    {
        container.BorderBottom(FrameRuleWidth).BorderColor(style.Primary).PaddingBottom(4).Row(row =>
        {
            row.ConstantItem(85).Height(28).AlignCenter().Element(logo => Logo(logo, style));
            row.RelativeItem().PaddingLeft(8).Column(column =>
            {
                column.Item().Text(title).Bold();
                column.Item().Text(details);
            });
            row.ConstantItem(82).AlignRight().Element(pages => PageNumber(pages, PageLabel, PageSeparator));
        });
    }

    public static void Footer(IContainer container, ReportStyle style, string text, string pageLabel = PageLabel, string pageSeparator = PageSeparator, float pageNumberWidth = 82)
    {
        container.BorderTop(FrameRuleWidth).BorderColor(style.Primary).PaddingTop(3).Row(row =>
        {
            row.RelativeItem().Text(text);
            row.ConstantItem(pageNumberWidth).AlignRight().Element(pages => PageNumber(pages, pageLabel, pageSeparator));
        });
    }

    private static void PageNumber(IContainer container, string label, string separator)
    {
        container.Text(text =>
        {
            text.Span(label);
            text.CurrentPageNumber();
            text.Span(separator);
            text.TotalPages();
        });
    }
}

public static class ReportHeaderComponent
{
    public static void Compose(IContainer container, ReportHeaderData data, ReportStyle style)
    {
        var culture = ReportFormatters.Culture(data.LanguageCode);
        ReportPage.Header(
            container,
            first => ComposeFirstPage(first, data, style, culture),
            compact => ReportPage.CompactHeader(
                compact,
                style,
                $"{data.Title}: {data.Number} - {data.HeaderDate}: {ReportFormatters.Date(data.Date, culture)}",
                $"{data.LeftParty.TaxNumber} - {data.RightParty.Name}"));
    }

    private static void ComposeFirstPage(IContainer container, ReportHeaderData data, ReportStyle style, System.Globalization.CultureInfo culture)
    {
        var table = new ReportTable(style);
        container.Column(column =>
        {
            column.Item().Height(72).Row(row =>
            {
                row.ConstantItem(150).Height(42).AlignMiddle().Element(logo => ReportPage.Logo(logo, style));
                row.RelativeItem();
                row.ConstantItem(195).Column(metadata =>
                {
                    metadata.Item().Text(data.Title).FontSize(14).Bold().FontColor(style.Primary);
                    metadata.Item().PaddingTop(8).Table(grid =>
                    {
                        grid.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(); });
                        grid.Header(header =>
                        {
                            header.Cell().Element(cell => table.MetadataCell(cell, data.HeaderNumber));
                            header.Cell().Element(cell => table.MetadataCell(cell, data.HeaderDate));
                        });
                        grid.Cell().Element(cell => ReportTable.MetadataValueCell(cell, data.Number));
                        grid.Cell().Element(cell => ReportTable.MetadataValueCell(cell, ReportFormatters.Date(data.Date, culture)));
                    });
                });
            });
            column.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Element(party => PartyComponent.Compose(party, data.LeftParty));
                row.ConstantItem(230).Element(party => PartyComponent.Compose(party, data.RightParty));
            });
            column.Item().PaddingTop(6).Element(rule => ReportPage.BrandRule(rule, style));
        });
    }
}

public static class PartyComponent
{
    public static void Compose(IContainer container, ReportParty party)
    {
        container.Column(column =>
        {
            column.Item().Text(party.Name).FontSize(10).SemiBold();
            foreach (var line in party.Lines.Where(line => !string.IsNullOrWhiteSpace(line)))
                column.Item().Text(line).FontSize(10);
            if (!string.IsNullOrWhiteSpace(party.TaxNumber))
                column.Item().Text($"NIF: {party.TaxNumber}").FontSize(10);
        });
    }
}

public static class ReportWatermark
{
    public static void Compose(IContainer container, byte[] watermark) =>
        container.AlignCenter().AlignMiddle().Width(260).Image(watermark).FitArea();
}

public static class ReportFooterComponent
{
    public static void Compose(IContainer container, ReportStyle style, string number, string issuerVat) =>
        ReportPage.Footer(container, style, $"Número: {number} - NIF: {issuerVat}");
}

public sealed class ReportTable(ReportStyle style)
{
    public void HeaderCell(IContainer container, string value) =>
        container.Background(style.TableHeaderFill).BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingVertical(3).PaddingHorizontal(4).Text(value).SemiBold();

    public void CaptionCell(IContainer container, string value) =>
        container.Background(style.AccentFill).Padding(4).Text(value).SemiBold();

    public void MetadataCell(IContainer container, string value) =>
        container.Background(style.AccentFill).Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).Text(value).SemiBold();

    public static void MetadataValueCell(IContainer container, string value) =>
        container.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).AlignCenter().Text(value);

    public static void BodyCell(IContainer container, string value, bool rightAligned = false)
    {
        container = container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);
        if (rightAligned) container = container.AlignRight();
        container.Text(value);
    }
}
