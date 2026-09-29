using Application.Contracts;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

public sealed class SalesOrderPdfService(IReportBrandingProvider brandingProvider) : ISalesOrderPdfService
{
    public async Task<byte[]> Generate(SalesOrderReportResponse report, CancellationToken cancellationToken = default)
    {
        var style = ReportStyle.From(await brandingProvider.GetCurrent(cancellationToken));
        return new SalesOrderDocument(report, style).GeneratePdf();
    }
}
