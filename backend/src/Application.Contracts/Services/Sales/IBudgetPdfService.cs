namespace Application.Contracts;

public interface IBudgetPdfService
{
    byte[] Generate(BudgetReportResponse report);
}