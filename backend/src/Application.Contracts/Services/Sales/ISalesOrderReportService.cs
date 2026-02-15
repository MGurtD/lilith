using Domain.Entities.Sales;

namespace Application.Contracts;

public interface ISalesOrderReportService
{
    Task<SalesOrderReportResponse?> GetReportById(Guid id, bool showPrices = true);
}
