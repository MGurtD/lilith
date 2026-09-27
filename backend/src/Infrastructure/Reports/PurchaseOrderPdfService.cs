using Application.Contracts;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

public sealed class PurchaseOrderPdfService(IReportBrandingProvider brandingProvider) : IPurchaseOrderPdfService
{
    public async Task<byte[]> Generate(PurchaseOrderReportResponse report, CancellationToken cancellationToken = default)
    {
        var style = ReportStyle.From(await brandingProvider.GetCurrent(cancellationToken));
        return new PurchaseOrderDocument(report, style).GeneratePdf();
    }
}
