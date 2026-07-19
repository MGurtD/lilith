namespace Application.Contracts;

public interface IPurchaseOrderPdfService
{
    byte[] Generate(PurchaseOrderReportResponse report);
}