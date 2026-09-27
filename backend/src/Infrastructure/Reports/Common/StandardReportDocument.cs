using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Infrastructure.Reports.Common.Components;

namespace Infrastructure.Reports.Common;

public abstract class StandardReportDocument : IDocument
{
    protected ReportHeaderData Header { get; }
    protected ReportStyle Style { get; }
    protected ReportTable Table { get; }
    protected System.Globalization.CultureInfo Culture { get; }
    private readonly string footerVat;

    protected StandardReportDocument(ReportHeaderData header, string footerVat, ReportStyle style)
    {
        Header = header;
        Style = style;
        Table = new ReportTable(style);
        Culture = ReportFormatters.Culture(header.LanguageCode);
        this.footerVat = footerVat;
    }

    public virtual DocumentMetadata GetMetadata() => Style.Metadata($"{Header.Title} {Header.Number}");

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            ReportPage.Configure(page, Style, ReportPageOptions.Standard);
            page.Header().Element(container => ReportHeaderComponent.Compose(container, Header, Style));
            page.Content().PaddingTop(10).Column(ComposeContent);
            page.Footer().Element(container => ReportFooterComponent.Compose(container, Style, Header.Number, footerVat));
        });
    }

    protected abstract void ComposeContent(ColumnDescriptor column);
}
