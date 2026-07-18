using Application.Contracts;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

/// <summary>
/// Keeps the QuestPDF dependency inside Infrastructure.
/// </summary>
public class SalesInvoicePdfService(IQrCodeService qrCodeService) : ISalesInvoicePdfService
{
    public byte[] Generate(InvoiceReportDto invoice)
    {
        var qrCodePng = string.IsNullOrWhiteSpace(invoice.QrCodeUrl)
            ? null
            : qrCodeService.GeneratePngBase64(invoice.QrCodeUrl);

        return new SalesInvoiceDocument(invoice, qrCodePng).GeneratePdf();
    }
}