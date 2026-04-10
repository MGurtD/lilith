using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Services.Production;

public class WorkOrderStockService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService,
    IWorkcenterLocationService workcenterLocationService,
    IWarehouseService warehouseService,
    IStockMovementService stockMovementService) : IWorkOrderStockService
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
            MovementType = StockMovementType.SUPPLY,
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
            MovementType = StockMovementType.SUPPLY,
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

    public async Task<GenericResponse> CreateProductionMovement(CreateProductionMovementRequest request)
    {
        // 1. Check if a production movement already exists for this WorkOrder
        var existingMovement = unitOfWork.StockMovements.Find(m =>
            m.Entity == StockMovementEntities.WorkOrder
            && m.EntityId == request.WorkOrderId
            && m.MovementType == StockMovementType.PRODUCTION).Any();
        if (existingMovement)
            return new GenericResponse(true);

        // 2. Get WorkOrder
        var workOrder = await unitOfWork.WorkOrders.Get(request.WorkOrderId);
        if (workOrder == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderNotFound", request.WorkOrderId));

        // 2. Get default warehouse location
        var defaultLocationId = await warehouseService.GetDefaultLocation();
        if (defaultLocationId == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockDefaultLocationNotFound"));

        // 3. Build the PRODUCTION stock movement
        var stockMovement = new StockMovement
        {
            ReferenceId = workOrder.ReferenceId,
            LocationId = defaultLocationId,
            MovementType = StockMovementType.PRODUCTION,
            Quantity = request.Quantity,
            MovementDate = DateTime.Now,
            Description = localizationService.GetLocalizedString("Movement.ProductionDescription", workOrder.Code),
            Entity = StockMovementEntities.WorkOrder,
            EntityId = workOrder.Id
        };

        // 4. Create stock movement (creates/updates Stock record and records the movement)
        return await stockMovementService.CreateProductionMovement(stockMovement);
    }
}
