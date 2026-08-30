using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Contracts
{
    public interface IStockMovementService
    {
        Task<GenericResponse> Create(StockMovement stockMovement);
        Task<GenericResponse> CreateProductionMovement(StockMovement stockMovement);

        /// <summary>
        /// Aplica en lot els moviments d'estoc d'un albarà (Deliver/UnDeliver).
        /// Carrega totes les referències i estocs en consultes conjuntes, acumula
        /// quantitats per a claus repetides (LocationId + ReferenceId + dimensions)
        /// i executa un únic CompleteAsync al final. Les referències de tipus Service
        /// es salten sense error ni moviment, igual que <see cref="Create"/>.
        /// </summary>
        Task<GenericResponse> ApplyDeliveryNoteStockBatch(IEnumerable<StockMovement> movements);

        IEnumerable<StockMovement> GetBetweenDates(DateTime startDate, DateTime endDate, Guid? locationId);
        Task<IEnumerable<StockMovement>> GetByWorkOrderId(Guid workOrderId);
        Task<GenericResponse> Remove(Guid id);
    }
}
