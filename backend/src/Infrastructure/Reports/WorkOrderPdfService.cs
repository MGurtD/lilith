using Application.Contracts;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

public sealed class WorkOrderPdfService : IWorkOrderPdfService
{
    public byte[] Generate(WorkOrderReportResponse report) => new WorkOrderDocument(report).GeneratePdf();
}