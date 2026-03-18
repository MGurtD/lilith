using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Services.Production;

public class WorkOrderStockService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService,
    IWorkcenterLocationService workcenterLocationService) : IWorkOrderStockService
{
    public async Task<GenericResponse> MoveToWorkcenterSupply(MoveStockToWorkcenterSupplyRequest request)
    {
        // 1. Validate and get source stock
        var sourceStock = await unitOfWork.Stocks.Get(request.StockId);
        if (sourceStock == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockNotFound"));

        if (sourceStock.Quantity < request.Quantity)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockInsufficientQuantity"));

        // 2. Find the Supply location for the workcenter
        var workcenterLocations = await workcenterLocationService.GetByWorkcenterId(request.WorkcenterId);
        var workcenterLocationsList = workcenterLocations.ToList();

        if (workcenterLocationsList.Count == 0)
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterSupplyLocationNotFound"));

        var supplyLocationId = workcenterLocationsList.First().LocationId;
        var supplyLocation = await unitOfWork.Warehouses.Locations.Get(supplyLocationId);

        if (supplyLocation == null || supplyLocation.Disabled)
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterSupplyLocationNotFound"));

        // 3. If the stock is already at the supply location, nothing to do
        if (sourceStock.LocationId == supplyLocation.Id)
            return new GenericResponse(true, sourceStock);

        var sourceLocationId = sourceStock.LocationId;
        var description = localizationService.GetLocalizedString("Movement.TransferToSupplyDescription", supplyLocation.Name);

        // 4. Relocate stock register
        sourceStock.LocationId = supplyLocation.Id;
        await unitOfWork.Stocks.Update(sourceStock);

        // 5. OUTPUT movement record (history)
        var outputMovement = new StockMovement
        {
            StockId = sourceStock.Id,
            LocationId = sourceLocationId,
            ReferenceId = sourceStock.ReferenceId,
            MovementType = StockMovementType.OUTPUT,
            Quantity = request.Quantity * -1,
            Width = sourceStock.Width,
            Length = sourceStock.Length,
            Height = sourceStock.Height,
            Diameter = sourceStock.Diameter,
            Thickness = sourceStock.Thickness,
            MovementDate = DateTime.UtcNow,
            Description = description,
            Entity = StockMovementEntities.WorkOrderPhase,
            EntityId = request.WorkOrderPhaseId
        };

        // 6. INPUT movement record (history)
        var inputMovement = new StockMovement
        {
            StockId = sourceStock.Id,
            LocationId = supplyLocation.Id,
            ReferenceId = sourceStock.ReferenceId,
            MovementType = StockMovementType.INPUT,
            Quantity = request.Quantity,
            Width = sourceStock.Width,
            Length = sourceStock.Length,
            Height = sourceStock.Height,
            Diameter = sourceStock.Diameter,
            Thickness = sourceStock.Thickness,
            MovementDate = DateTime.UtcNow,
            Description = description,
            Entity = StockMovementEntities.WorkOrderPhase,
            EntityId = request.WorkOrderPhaseId
        };

        await unitOfWork.StockMovements.AddRange([outputMovement, inputMovement]);
        return new GenericResponse(true, sourceStock);
    }
}
