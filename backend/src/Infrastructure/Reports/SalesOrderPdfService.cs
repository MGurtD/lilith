using Application.Contracts;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

public sealed class SalesOrderPdfService : ISalesOrderPdfService
{
    public byte[] Generate(SalesOrderReportResponse report) => new SalesOrderDocument(report).GeneratePdf();
}