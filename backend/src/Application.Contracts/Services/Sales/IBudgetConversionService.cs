namespace Application.Contracts
{
    public interface IBudgetConversionService
    {
        Task<BudgetConversionResult> GetConversion(DateTime startDate, DateTime endDate, Guid? customerId);
    }
}
