using Domain.Entities.Warehouse;

namespace Application.Contracts;

public interface IWorkOrderStockService
{
    Task<GenericResponse> MoveToWorkcenterSupply(MoveStockToWorkcenterSupplyRequest request);
    Task<GenericResponse> ReturnFromWorkcenterSupply(ReturnStockFromSupplyRequest request);
    Task<GenericResponse> CreateProductionMovement(CreateProductionMovementRequest request);
    Task<GenericResponse> ConsumePhaseStock(ConsumePhaseStockRequest request);
    IEnumerable<StockMovement> GetPhaseConsumptions(Guid workOrderPhaseId);
}
