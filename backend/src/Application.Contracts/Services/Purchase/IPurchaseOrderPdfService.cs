namespace Application.Contracts;

public interface IPurchaseOrderPdfService
{
    Task<byte[]> Generate(PurchaseOrderReportResponse report, CancellationToken cancellationToken = default);
}