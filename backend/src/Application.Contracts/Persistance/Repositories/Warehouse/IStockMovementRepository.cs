using Domain.Entities.Warehouse;

namespace Application.Contracts
{
    public interface IStockMovementRepository : IRepository<StockMovement, Guid>
    {
        IEnumerable<StockMovement> GetBetweenDatesWithLocation(DateTime startDate, DateTime endDate, Guid? locationId);
        IEnumerable<StockMovement> GetByEntityReferences(Guid workOrderId, IEnumerable<Guid> workOrderPhaseIds);
    }
}
