using Application.Contracts;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

public sealed class DeliveryNotePdfService : IDeliveryNotePdfService
{
    public byte[] Generate(DeliveryNoteReportResponse report) => new DeliveryNoteDocument(report).GeneratePdf();
}