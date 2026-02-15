using Application.Contracts;
using Application.Services;
using Application.Services.Sales;
using Domain.Entities.Sales;

namespace Application.Services.Sales
{
    public class BudgetReportService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IBudgetReportService
    {
        public async Task<Application.Contracts.BudgetReportResponse?> GetReportById(Guid id)
        {
            var budget = await unitOfWork.Budgets.Get(id);
            if (budget is null) return null;

            var customer = await unitOfWork.Customers.Get(budget.CustomerId);
            if (customer is null) return null;

            var site = (await unitOfWork.Sites.FindAsync(s => !s.Disabled)).FirstOrDefault();
            if (site == null) return null;

            budget.Details = budget.Details.OrderBy(d => d.Reference!.Code).ToList();
            var report = new BudgetReportResponse(customer.PreferredLanguage, localizationService)
            {
                Budget = budget,
                Customer = customer,
                Site = site,
                Total = budget.Details.Sum(d => d.Amount),
            };
            return report;
        }
    }
}






