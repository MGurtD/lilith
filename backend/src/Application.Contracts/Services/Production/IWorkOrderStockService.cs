using Domain.Entities.Warehouse;

namespace Application.Contracts;

public interface IWorkOrderStockService
{
    Task<GenericResponse> MoveToWorkcenterSupply(MoveStockToWorkcenterSupplyRequest request);
    Task<GenericResponse> CreateProductionMovement(CreateProductionMovementRequest request);
}
