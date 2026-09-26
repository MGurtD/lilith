using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Infrastructure.Reports.Common.Components;

namespace Infrastructure.Reports.Common;

public abstract class StandardReportDocument : IDocument
{
    protected ReportHeaderData Header { get; }
    protected System.Globalization.CultureInfo Culture { get; }
    private readonly string footerVat;

    protected StandardReportDocument(ReportHeaderData header, string footerVat)
    {
        Header = header;
        Culture = ReportFormatters.Culture(header.LanguageCode);
        this.footerVat = footerVat;
    }

    public virtual DocumentMetadata GetMetadata() => new() { Title = $"{Header.Title} {Header.Number}", Author = "TEMGES" };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(ReportTheme.PageMargin);
            page.DefaultTextStyle(style => style.FontSize(9));
            page.Foreground().Element(ReportWatermark.Compose);
            page.Header().Element(container => ReportHeaderComponent.Compose(container, Header));
            page.Content().PaddingTop(10).Column(ComposeContent);
            page.Footer().Element(container => ReportFooterComponent.Compose(container, Header.Number, footerVat));
        });
    }

    protected abstract void ComposeContent(ColumnDescriptor column);
}