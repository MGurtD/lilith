using System;
using System.Threading.Tasks;
using Application.Contracts;

namespace Application.Contracts;

public interface IPurchaseOrderReportService
{
    Task<PurchaseOrderReportResponse?> GetReportById(Guid id);
}
