using Application.Contracts;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

/// <summary>
/// Keeps the QuestPDF dependency inside Infrastructure.
/// </summary>
public class SalesInvoicePdfService(IQrCodeService qrCodeService, IReportBrandingProvider brandingProvider) : ISalesInvoicePdfService
{
    public async Task<byte[]> Generate(InvoiceReportDto invoice, CancellationToken cancellationToken = default)
    {
        var qrCodePng = string.IsNullOrWhiteSpace(invoice.QrCodeUrl)
            ? null
            : qrCodeService.GeneratePngBase64(invoice.QrCodeUrl);
        var style = ReportStyle.From(await brandingProvider.GetCurrent(cancellationToken));

        return new SalesInvoiceDocument(invoice, qrCodePng, style).GeneratePdf();
    }
}
