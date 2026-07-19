using Application.Contracts;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

public sealed class BudgetPdfService : IBudgetPdfService
{
    public byte[] Generate(BudgetReportResponse report) => new BudgetDocument(report).GeneratePdf();
}