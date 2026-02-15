namespace Application.Contracts;

public interface IIncomeService
{
    IEnumerable<ConsolidatedIncomes> GetConsolidatedBetweenDates(DateTime startTime, DateTime endTime);
}
