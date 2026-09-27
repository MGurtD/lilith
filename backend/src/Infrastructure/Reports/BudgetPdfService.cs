using Application.Contracts;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

public sealed class BudgetPdfService(IReportBrandingProvider brandingProvider) : IBudgetPdfService
{
    public async Task<byte[]> Generate(BudgetReportResponse report, CancellationToken cancellationToken = default)
    {
        var style = ReportStyle.From(await brandingProvider.GetCurrent(cancellationToken));
        return new BudgetDocument(report, style).GeneratePdf();
    }
}
