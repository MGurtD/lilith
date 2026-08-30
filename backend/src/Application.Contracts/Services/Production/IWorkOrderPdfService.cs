namespace Application.Contracts;

public interface IWorkOrderPdfService
{
    byte[] Generate(WorkOrderReportResponse report);
}
