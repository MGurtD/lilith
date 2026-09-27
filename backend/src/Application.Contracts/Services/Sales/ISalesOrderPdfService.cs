namespace Application.Contracts;

public interface ISalesOrderPdfService
{
    Task<byte[]> Generate(SalesOrderReportResponse report, CancellationToken cancellationToken = default);
}