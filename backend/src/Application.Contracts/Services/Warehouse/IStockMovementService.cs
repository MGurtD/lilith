using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Contracts
{
    public interface IStockMovementService
    {
        Task<GenericResponse> Create(StockMovement stockMovement);
        Task<GenericResponse> CreateProductionMovement(StockMovement stockMovement);
        IEnumerable<StockMovement> GetBetweenDates(DateTime startDate, DateTime endDate, Guid? locationId);
        Task<IEnumerable<StockMovement>> GetByWorkOrderId(Guid workOrderId);
        Task<GenericResponse> Remove(Guid id);
    }
}
