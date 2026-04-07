using Application.Contracts;
using Domain.Entities.Warehouse;
using Domain.Implementations.ReferenceFormat;

namespace Application.Services.Production;

public class WorkOrderStockService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService,
    IWorkcenterLocationService workcenterLocationService,
    IWarehouseService warehouseService,
    IStockMovementService stockMovementService,
    IStockService stockService) : IWorkOrderStockService
{
    public async Task<GenericResponse> MoveToWorkcenterSupply(MoveStockToWorkcenterSupplyRequest request)
    {
        // 1. Validate quantity
        if (request.Quantity <= 0)
            return new GenericResponse(false, localizationService.GetLocalizedString("QuantityMustBeGreaterThanZero"));

        // 2. Validate and get source stock
        var sourceStock = await unitOfWork.Stocks.Get(request.StockId);
        if (sourceStock == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockNotFound"));

        if (sourceStock.Quantity < request.Quantity)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockInsufficientQuantity"));

        // 3. Find the Supply location for the workcenter
        var workcenterLocations = await workcenterLocationService.GetByWorkcenterId(request.WorkcenterId);
        var workcenterLocationsList = workcenterLocations.ToList();

        if (workcenterLocationsList.Count == 0)
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterSupplyLocationNotFound"));

        var supplyLocationId = workcenterLocationsList.First().LocationId;
        var supplyLocation = await unitOfWork.Warehouses.Locations.Get(supplyLocationId);

        if (supplyLocation == null || supplyLocation.Disabled)
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterSupplyLocationNotFound"));

        // 4. If the stock is already at the supply location, nothing to do
        if (sourceStock.LocationId == supplyLocation.Id)
            return new GenericResponse(true, sourceStock);

        var sourceLocationId = sourceStock.LocationId;
        var sourceStockId = sourceStock.Id;
        var description = localizationService.GetLocalizedString("Movement.TransferToSupplyDescription", supplyLocation.Name);
        var isFullMove = request.Quantity == sourceStock.Quantity;

        // 5. Check if stock with same dimensions already exists at supply location
        var existingDestinationStock = stockService.GetByDimensions(
            supplyLocation.Id,
            sourceStock.ReferenceId,
            sourceStock.Width,
            sourceStock.Length,
            sourceStock.Height,
            sourceStock.Diameter,
            sourceStock.Thickness);

        Guid destinationStockId;

        if (isFullMove && existingDestinationStock == null)
        {
            // Full move, no existing stock at destination: relocate the entire record
            sourceStock.LocationId = supplyLocation.Id;
            await unitOfWork.Stocks.Update(sourceStock);
            destinationStockId = sourceStock.Id;
        }
        else
        {
            // Partial move or merge into existing destination stock
            // Reduce source stock quantity
            sourceStock.Quantity -= request.Quantity;

            if (sourceStock.Quantity == 0)
            {
                // Source depleted: keep record with quantity 0 and relocate to default location
                // to preserve FK integrity with existing StockMovements
                var defaultLocationId = await warehouseService.GetDefaultLocation();
                if (defaultLocationId != null)
                {
                    sourceStock.LocationId = defaultLocationId.Value;
                }
            }

            await unitOfWork.Stocks.Update(sourceStock);

            if (existingDestinationStock != null)
            {
                // Merge into existing stock at supply location
                existingDestinationStock.Quantity += request.Quantity;
                await unitOfWork.Stocks.Update(existingDestinationStock);
                destinationStockId = existingDestinationStock.Id;
            }
            else
            {
                // Create new stock record at supply location
                var newStock = new Stock
                {
                    ReferenceId = sourceStock.ReferenceId,
                    LocationId = supplyLocation.Id,
                    Quantity = request.Quantity,
                    Width = sourceStock.Width,
                    Length = sourceStock.Length,
                    Height = sourceStock.Height,
                    Diameter = sourceStock.Diameter,
                    Thickness = sourceStock.Thickness
                };
                await unitOfWork.Stocks.Add(newStock);
                destinationStockId = newStock.Id;
            }
        }

        // 6. OUTPUT movement record (history) - negative quantity at source location
        var outputMovement = new StockMovement
        {
            StockId = sourceStockId,
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

        // 7. INPUT movement record (history) - positive quantity at supply location
        var inputMovement = new StockMovement
        {
            StockId = destinationStockId,
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
        return new GenericResponse(true);
    }

    public async Task<GenericResponse> ReturnFromWorkcenterSupply(ReturnStockFromSupplyRequest request)
    {
        // 1. Validate quantity
        if (request.Quantity <= 0)
            return new GenericResponse(false, localizationService.GetLocalizedString("QuantityMustBeGreaterThanZero"));

        // 2. Validate and get source stock
        var sourceStock = await unitOfWork.Stocks.Get(request.StockId);
        if (sourceStock == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockNotFound"));

        if (sourceStock.Quantity < request.Quantity)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockInsufficientQuantity"));

        // 3. Find the Supply location for the workcenter
        var workcenterLocations = await workcenterLocationService.GetByWorkcenterId(request.WorkcenterId);
        var workcenterLocationIds = workcenterLocations.Select(wl => wl.LocationId).ToHashSet();

        if (workcenterLocationIds.Count == 0)
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterSupplyLocationNotFound"));

        // 4. Validate the stock is at a workcenter supply location
        if (!workcenterLocationIds.Contains(sourceStock.LocationId))
            return new GenericResponse(false, localizationService.GetLocalizedString("StockNotAtSupplyLocation"));

        // 5. Get default warehouse location as destination
        var defaultLocationId = await warehouseService.GetDefaultLocation();
        if (defaultLocationId == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockDefaultLocationNotFound"));

        var defaultLocation = await unitOfWork.Warehouses.Locations.Get(defaultLocationId.Value);
        if (defaultLocation == null || defaultLocation.Disabled)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockDefaultLocationNotFound"));

        var supplyLocationId = sourceStock.LocationId;
        var sourceStockId = sourceStock.Id;
        var description = localizationService.GetLocalizedString("Movement.ReturnFromSupplyDescription", defaultLocation.Name);
        var isFullReturn = request.Quantity == sourceStock.Quantity;

        // 6. Check if stock with same dimensions already exists at default location
        var existingDestinationStock = stockService.GetByDimensions(
            defaultLocation.Id,
            sourceStock.ReferenceId,
            sourceStock.Width,
            sourceStock.Length,
            sourceStock.Height,
            sourceStock.Diameter,
            sourceStock.Thickness);

        Guid destinationStockId;

        if (isFullReturn && existingDestinationStock == null)
        {
            // Full return, no existing stock at destination: relocate the entire record
            sourceStock.LocationId = defaultLocation.Id;
            await unitOfWork.Stocks.Update(sourceStock);
            destinationStockId = sourceStock.Id;
        }
        else
        {
            // Partial return or merge into existing destination stock
            sourceStock.Quantity -= request.Quantity;

            if (sourceStock.Quantity == 0)
            {
                // Source depleted: keep record with quantity 0 at default location
                // to preserve FK integrity with existing StockMovements
                sourceStock.LocationId = defaultLocation.Id;
            }

            await unitOfWork.Stocks.Update(sourceStock);

            if (existingDestinationStock != null)
            {
                existingDestinationStock.Quantity += request.Quantity;
                await unitOfWork.Stocks.Update(existingDestinationStock);
                destinationStockId = existingDestinationStock.Id;
            }
            else
            {
                var newStock = new Stock
                {
                    ReferenceId = sourceStock.ReferenceId,
                    LocationId = defaultLocation.Id,
                    Quantity = request.Quantity,
                    Width = sourceStock.Width,
                    Length = sourceStock.Length,
                    Height = sourceStock.Height,
                    Diameter = sourceStock.Diameter,
                    Thickness = sourceStock.Thickness
                };
                await unitOfWork.Stocks.Add(newStock);
                destinationStockId = newStock.Id;
            }
        }

        // 7. OUTPUT movement record (history) - negative quantity at supply location
        var outputMovement = new StockMovement
        {
            StockId = sourceStockId,
            LocationId = supplyLocationId,
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

        // 8. INPUT movement record (history) - positive quantity at default location
        var inputMovement = new StockMovement
        {
            StockId = destinationStockId,
            LocationId = defaultLocation.Id,
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
        return new GenericResponse(true);
    }

    public async Task<GenericResponse> ConsumePhaseStock(ConsumePhaseStockRequest request)
    {
        // 1. Validate request
        if (request.ConsumedItems == null || request.ConsumedItems.Count == 0)
            return new GenericResponse(false, localizationService.GetLocalizedString("Movement.Consumption.NoItems"));

        // 2. Get workcenter supply location IDs
        var workcenterLocations = await workcenterLocationService.GetByWorkcenterId(request.WorkcenterId);
        var workcenterLocationIds = workcenterLocations.Select(wl => wl.LocationId).ToHashSet();

        if (workcenterLocationIds.Count == 0)
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterSupplyLocationNotFound"));

        // 3. Get default warehouse location for auto-return of unconsumed stock
        var defaultLocationId = await warehouseService.GetDefaultLocation();
        if (defaultLocationId == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockDefaultLocationNotFound"));

        var defaultLocation = await unitOfWork.Warehouses.Locations.Get(defaultLocationId.Value);
        if (defaultLocation == null || defaultLocation.Disabled)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockDefaultLocationNotFound"));

        var allMovements = new List<StockMovement>();

        // 4. Process each consumed item
        foreach (var item in request.ConsumedItems)
        {
            if (item.Quantity <= 0)
                return new GenericResponse(false, localizationService.GetLocalizedString("QuantityMustBeGreaterThanZero"));

            var sourceStock = await unitOfWork.Stocks.Get(item.StockId);
            if (sourceStock == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("StockNotFound"));

            // Validate stock is at a workcenter supply location
            if (!workcenterLocationIds.Contains(sourceStock.LocationId))
                return new GenericResponse(false, localizationService.GetLocalizedString("StockNotAtSupplyLocation"));

            if (sourceStock.Quantity < item.Quantity)
                return new GenericResponse(false, localizationService.GetLocalizedString("StockInsufficientQuantity"));

            var sourceLocationId = sourceStock.LocationId;
            var description = localizationService.GetLocalizedString("Movement.ConsumptionDescription");

            // Reduce source stock quantity
            sourceStock.Quantity -= item.Quantity;

            if (sourceStock.Quantity == 0)
            {
                // Source depleted: keep record with quantity 0 at default location
                // to preserve FK integrity with existing StockMovements
                sourceStock.LocationId = defaultLocation.Id;
            }

            await unitOfWork.Stocks.Update(sourceStock);

            // Create CONSUMPTION movement for consumed stock leaving the supply location
            var consumptionMovement = new StockMovement
            {
                StockId = sourceStock.Id,
                LocationId = sourceLocationId,
                ReferenceId = sourceStock.ReferenceId,
                MovementType = StockMovementType.CONSUMPTION,
                Quantity = item.Quantity * -1,
                Width = item.Width,
                Length = item.Length,
                Height = item.Height,
                Diameter = item.Diameter,
                Thickness = item.Thickness,
                MovementDate = DateTime.UtcNow,
                Description = description,
                Entity = StockMovementEntities.WorkOrderPhase,
                EntityId = request.WorkOrderPhaseId
            };

            allMovements.Add(consumptionMovement);

            // 4b. Calculate dimensional residue per consumed item.
            //     If consumed dimensions are smaller than the source stock dimensions,
            //     the leftover piece (residue) is returned to the default location
            //     without subtracting cutting waste.
            var residueWidth = sourceStock.Width;
            var residueLength = sourceStock.Length;
            var residueHeight = sourceStock.Height;
            var residueDiameter = sourceStock.Diameter;
            var residueThickness = sourceStock.Thickness;
            var hasResidue = false;

            // Get the reference format to determine the cutting axis
            var reference = await unitOfWork.References.Get(sourceStock.ReferenceId);
            if (reference?.ReferenceFormatId != null)
            {
                var format = await unitOfWork.ReferenceFormats.Get(reference.ReferenceFormatId.Value);
                if (format != null)
                {
                    var formatCode = format.Code;

                    if (formatCode == ReferenceFormatCodes.RODO || formatCode == ReferenceFormatCodes.TUB)
                    {
                        // Cutting axis: length. Diameter (and thickness for TUB) stay the same.
                        if (item.Length < sourceStock.Length && item.Length > 0)
                        {
                            residueLength = sourceStock.Length - item.Length;
                            hasResidue = residueLength > 0;
                        }
                    }
                    else if (formatCode == ReferenceFormatCodes.PLACA)
                    {
                        // PLACA: check all 3 axes (width, length, height)
                        if (item.Width < sourceStock.Width && item.Width > 0)
                        {
                            residueWidth = sourceStock.Width - item.Width;
                            if (residueWidth > 0) hasResidue = true;
                            else residueWidth = 0;
                        }
                        if (item.Length < sourceStock.Length && item.Length > 0)
                        {
                            residueLength = sourceStock.Length - item.Length;
                            if (residueLength > 0) hasResidue = true;
                            else residueLength = 0;
                        }
                        if (item.Height < sourceStock.Height && item.Height > 0)
                        {
                            residueHeight = sourceStock.Height - item.Height;
                            if (residueHeight > 0) hasResidue = true;
                            else residueHeight = 0;
                        }
                    }
                    // UNITATS: no dimensional residue
                }
            }

            if (hasResidue)
            {
                var returnDescription = localizationService.GetLocalizedString("Movement.ReturnFromSupplyDescription", defaultLocation.Name);

                // Find or create stock at default location with residue dimensions
                var existingResidueStock = stockService.GetByDimensions(
                    defaultLocation.Id,
                    sourceStock.ReferenceId,
                    residueWidth,
                    residueLength,
                    residueHeight,
                    residueDiameter,
                    residueThickness);

                Guid residueStockId;

                if (existingResidueStock != null)
                {
                    existingResidueStock.Quantity += item.Quantity;
                    await unitOfWork.Stocks.Update(existingResidueStock);
                    residueStockId = existingResidueStock.Id;
                }
                else
                {
                    var newResidueStock = new Stock
                    {
                        ReferenceId = sourceStock.ReferenceId,
                        LocationId = defaultLocation.Id,
                        Quantity = item.Quantity,
                        Width = residueWidth,
                        Length = residueLength,
                        Height = residueHeight,
                        Diameter = residueDiameter,
                        Thickness = residueThickness
                    };
                    await unitOfWork.Stocks.Add(newResidueStock);
                    residueStockId = newResidueStock.Id;
                }

                // INPUT movement for residue arriving at default location
                allMovements.Add(new StockMovement
                {
                    StockId = residueStockId,
                    LocationId = defaultLocation.Id,
                    ReferenceId = sourceStock.ReferenceId,
                    MovementType = StockMovementType.INPUT,
                    Quantity = item.Quantity,
                    Width = residueWidth,
                    Length = residueLength,
                    Height = residueHeight,
                    Diameter = residueDiameter,
                    Thickness = residueThickness,
                    MovementDate = DateTime.UtcNow,
                    Description = returnDescription,
                    Entity = StockMovementEntities.WorkOrderPhase,
                    EntityId = request.WorkOrderPhaseId
                });
            }
        }

        // 5. Auto-return: find remaining stock at workcenter supply locations
        //    scoped to the BOM references of the finalized phase only
        var phaseBom = unitOfWork.WorkOrders.Phases.BillOfMaterials
            .Find(b => b.WorkOrderPhaseId == request.WorkOrderPhaseId)
            .ToList();
        var phaseReferenceIds = phaseBom.Select(b => b.ReferenceId).ToHashSet();

        var allSupplyStock = new List<Stock>();
        foreach (var locationId in workcenterLocationIds)
        {
            var stockAtLocation = unitOfWork.Stocks.Find(s =>
                s.LocationId == locationId
                && s.Quantity > 0
                && phaseReferenceIds.Contains(s.ReferenceId)).ToList();
            allSupplyStock.AddRange(stockAtLocation);
        }

        foreach (var remainingStock in allSupplyStock)
        {
            var returnDescription = localizationService.GetLocalizedString("Movement.ReturnFromSupplyDescription", defaultLocation.Name);
            var returnSourceLocationId = remainingStock.LocationId;
            var returnQuantity = remainingStock.Quantity;

            var existingDestinationStock = stockService.GetByDimensions(
                defaultLocation.Id,
                remainingStock.ReferenceId,
                remainingStock.Width,
                remainingStock.Length,
                remainingStock.Height,
                remainingStock.Diameter,
                remainingStock.Thickness);

            Guid returnDestinationStockId;

            if (existingDestinationStock == null)
            {
                // No existing stock at destination: relocate the entire record
                remainingStock.LocationId = defaultLocation.Id;
                await unitOfWork.Stocks.Update(remainingStock);
                returnDestinationStockId = remainingStock.Id;
            }
            else
            {
                // Merge into existing stock at default location
                existingDestinationStock.Quantity += returnQuantity;
                await unitOfWork.Stocks.Update(existingDestinationStock);
                returnDestinationStockId = existingDestinationStock.Id;

                // Keep source record with quantity 0 at default location
                remainingStock.Quantity = 0;
                remainingStock.LocationId = defaultLocation.Id;
                await unitOfWork.Stocks.Update(remainingStock);
            }

            // OUTPUT movement (leaving supply location)
            allMovements.Add(new StockMovement
            {
                StockId = remainingStock.Id,
                LocationId = returnSourceLocationId,
                ReferenceId = remainingStock.ReferenceId,
                MovementType = StockMovementType.OUTPUT,
                Quantity = returnQuantity * -1,
                Width = remainingStock.Width,
                Length = remainingStock.Length,
                Height = remainingStock.Height,
                Diameter = remainingStock.Diameter,
                Thickness = remainingStock.Thickness,
                MovementDate = DateTime.UtcNow,
                Description = returnDescription,
                Entity = StockMovementEntities.WorkOrderPhase,
                EntityId = request.WorkOrderPhaseId
            });

            // INPUT movement (arriving at default location)
            allMovements.Add(new StockMovement
            {
                StockId = returnDestinationStockId,
                LocationId = defaultLocation.Id,
                ReferenceId = remainingStock.ReferenceId,
                MovementType = StockMovementType.INPUT,
                Quantity = returnQuantity,
                Width = remainingStock.Width,
                Length = remainingStock.Length,
                Height = remainingStock.Height,
                Diameter = remainingStock.Diameter,
                Thickness = remainingStock.Thickness,
                MovementDate = DateTime.UtcNow,
                Description = returnDescription,
                Entity = StockMovementEntities.WorkOrderPhase,
                EntityId = request.WorkOrderPhaseId
            });
        }

        // 6. Save all movements
        if (allMovements.Count > 0)
        {
            await unitOfWork.StockMovements.AddRange(allMovements);
        }

        return new GenericResponse(true);
    }

    public IEnumerable<StockMovement> GetPhaseConsumptions(Guid workOrderPhaseId)
    {
        return unitOfWork.StockMovements.Find(m =>
            m.Entity == StockMovementEntities.WorkOrderPhase
            && m.EntityId == workOrderPhaseId
            && m.MovementType == StockMovementType.CONSUMPTION);
    }

    public async Task<GenericResponse> CreateProductionMovement(CreateProductionMovementRequest request)
    {
        // 1. Get WorkOrder
        var workOrder = await unitOfWork.WorkOrders.Get(request.WorkOrderId);
        if (workOrder == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderNotFound", request.WorkOrderId));

        // 2. Check if a production movement already exists for this WorkOrder
        var existingMovement = unitOfWork.StockMovements.Find(m =>
            m.Entity == StockMovementEntities.WorkOrder
            && m.EntityId == request.WorkOrderId
            && m.MovementType == StockMovementType.PRODUCTION).Any();
        if (existingMovement)
            return new GenericResponse(true);

        // 3. Get default warehouse location
        var defaultLocationId = await warehouseService.GetDefaultLocation();
        if (defaultLocationId == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockDefaultLocationNotFound"));

        // 4. Build the PRODUCTION stock movement
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

        // 5. Create stock movement (creates/updates Stock record and records the movement)
        return await stockMovementService.CreateProductionMovement(stockMovement);
    }
}
