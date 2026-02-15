using System;
using System.Threading.Tasks;
using Application.Contracts;

namespace Application.Contracts;

public interface IWorkOrderReportService
{
    Task<WorkOrderReportResponse?> GetReportById(Guid id);
}
