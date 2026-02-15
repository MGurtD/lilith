using Application.Contracts;

namespace Application.Services.Sales;

public class IncomeService(IUnitOfWork unitOfWork) : IIncomeService
{
    public IEnumerable<ConsolidatedIncomes> GetConsolidatedBetweenDates(DateTime startTime, DateTime endTime)
    {
        return unitOfWork.ConsolidatedIncomes
            .Find(c => c.Date >= startTime && c.Date <= endTime);
    }
}
