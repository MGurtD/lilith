using System;
using System.Threading.Tasks;
using Domain.Entities.Sales;

namespace Application.Contracts;

public interface IBudgetReportService
{
    Task<BudgetReportResponse?> GetReportById(Guid id);
}
