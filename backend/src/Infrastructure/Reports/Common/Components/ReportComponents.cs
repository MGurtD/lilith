using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.Common.Components;

public sealed record ReportParty(string Name, IReadOnlyList<string> Lines, string TaxNumber);
public sealed record ReportHeaderData(string Title, string Number, DateTime Date, string HeaderNumber, string HeaderDate, ReportParty LeftParty, ReportParty RightParty, string LanguageCode);

public static class ReportHeaderComponent
{
    public static void Compose(IContainer container, ReportHeaderData data)
    {
        var culture = ReportFormatters.Culture(data.LanguageCode);
        container.Column(column =>
        {
            column.Item().ShowOnce().Element(first => ComposeFirstPage(first, data, culture));
            column.Item().SkipOnce().Element(compact => ComposeCompact(compact, data, culture));
        });
    }

    private static void ComposeFirstPage(IContainer container, ReportHeaderData data, System.Globalization.CultureInfo culture)
    {
        container.Column(column =>
        {
            column.Item().Height(72).Row(row =>
            {
                row.ConstantItem(150).Height(42).AlignMiddle().Image(ReportAssets.Logo).FitArea();
                row.RelativeItem();
                row.ConstantItem(195).Column(metadata =>
                {
                    metadata.Item().Text(data.Title).FontSize(14).Bold();
                    metadata.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(); });
                        table.Header(header =>
                        {
                            header.Cell().Element(cell => MetadataCell(cell, data.HeaderNumber));
                            header.Cell().Element(cell => MetadataCell(cell, data.HeaderDate));
                        });
                        table.Cell().Element(cell => MetadataValueCell(cell, data.Number));
                        table.Cell().Element(cell => MetadataValueCell(cell, ReportFormatters.Date(data.Date, culture)));
                    });
                });
            });
            column.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Element(party => PartyComponent.Compose(party, data.LeftParty));
                row.ConstantItem(230).Element(party => PartyComponent.Compose(party, data.RightParty));
            });
        });
    }

    private static void ComposeCompact(IContainer container, ReportHeaderData data, System.Globalization.CultureInfo culture)
    {
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(4).Row(row =>
        {
            row.ConstantItem(85).Height(28).AlignCenter().Image(ReportAssets.Logo).FitArea();
            row.RelativeItem().PaddingLeft(8).Column(details =>
            {
                details.Item().Text($"{data.Title}: {data.Number} - {data.HeaderDate}: {ReportFormatters.Date(data.Date, culture)}").Bold();
                details.Item().Text($"{data.LeftParty.TaxNumber} - {data.RightParty.Name}");
            });
            row.ConstantItem(82).AlignRight().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }

    private static void MetadataCell(IContainer container, string value) => container.Background(ReportTheme.Accent).Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).Text(value).SemiBold();
    private static void MetadataValueCell(IContainer container, string value) => container.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).AlignCenter().Text(value);
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
    public static void Compose(IContainer container) =>
        container.AlignCenter().AlignMiddle().Width(260).Image(ReportAssets.Watermark).FitArea();
}

public static class ReportFooterComponent
{
    public static void Compose(IContainer container, string number, string issuerVat)
    {
        container.BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(3).Row(row =>
        {
            row.RelativeItem().Text($"Número: {number} - NIF: {issuerVat}");
            row.ConstantItem(82).AlignRight().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }
}

public static class ReportTable
{
    public static void HeaderCell(IContainer container, string value) => container.Background(ReportTheme.TableHeader).BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingVertical(3).PaddingHorizontal(4).Text(value).SemiBold();
    public static void BodyCell(IContainer container, string value, bool rightAligned = false)
    {
        container = container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);
        if (rightAligned) container = container.AlignRight();
        container.Text(value);
    }
}