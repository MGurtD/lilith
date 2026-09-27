namespace Application.Contracts;

public interface IDeliveryNotePdfService
{
    Task<byte[]> Generate(DeliveryNoteReportResponse report, CancellationToken cancellationToken = default);
}