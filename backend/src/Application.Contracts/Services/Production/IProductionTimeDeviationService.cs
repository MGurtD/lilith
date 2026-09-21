namespace Application.Contracts
{
    public interface IProductionTimeDeviationService
    {
        Task<ProductionTimeDeviationResult> GetDeviation(DateTime startDate, DateTime endDate, Guid? workOrderId);
    }
}
