namespace Application.Contracts;

public interface IWorkOrderPdfService
{
    Task<byte[]> Generate(WorkOrderReportResponse report, CancellationToken cancellationToken = default);
}
