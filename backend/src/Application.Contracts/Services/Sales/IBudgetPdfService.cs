namespace Application.Contracts;

public interface IBudgetPdfService
{
    Task<byte[]> Generate(BudgetReportResponse report, CancellationToken cancellationToken = default);
}