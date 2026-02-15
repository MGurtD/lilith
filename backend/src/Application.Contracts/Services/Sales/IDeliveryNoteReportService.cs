using Domain.Entities.Sales;

namespace Application.Contracts;

public interface IDeliveryNoteReportService
{
    Task<DeliveryNoteReportResponse?> GetReportById(Guid id, bool showPrices = true);
}
