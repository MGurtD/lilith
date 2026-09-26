using Application.Contracts;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

public sealed class PurchaseOrderPdfService : IPurchaseOrderPdfService
{
    public byte[] Generate(PurchaseOrderReportResponse report) => new PurchaseOrderDocument(report).GeneratePdf();
}