namespace Application.Contracts;

public interface ISalesOrderPdfService
{
    byte[] Generate(SalesOrderReportResponse report);
}