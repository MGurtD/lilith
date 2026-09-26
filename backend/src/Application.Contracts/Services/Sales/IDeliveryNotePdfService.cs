namespace Application.Contracts;

public interface IDeliveryNotePdfService
{
    byte[] Generate(DeliveryNoteReportResponse report);
}