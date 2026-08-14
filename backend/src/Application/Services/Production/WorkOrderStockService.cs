using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Services.Production;

public class WorkOrderStockService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService,
    IWorkcenterLocationService workcenterLocationService,
    IWarehouseService warehouseService,
    IStockMovementService stockMovementService,
    IStockService stockService,
    ILotService lotService) : IWorkOrderStockService
{
    /// <summary>
    /// Resolves the WorkOrder.Code from a WorkOrderPhaseId.
    /// Returns empty string if the phase or work order is not found.
    /// </summary>
    private async Task<string> GetWorkOrderCodeByPhaseId(Guid workOrderPhaseId)
    {
        var phase = await unitOfWork.WorkOrders.Phases.Get(workOrderPhaseId);
        if (phase == null) return string.Empty;

        var workOrder = await unitOfWork.WorkOrders.Get(phase.WorkOrderId);
        return workOrder?.Code ?? string.Empty;
    }

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
        var isFullMove = request.Quantity == sourceStock.Quantity;

        // Resolve WO code and source location name for descriptions
        var woCode = await GetWorkOrderCodeByPhaseId(request.WorkOrderPhaseId);
        var sourceLocation = await unitOfWork.Warehouses.Locations.Get(sourceLocationId);
        var sourceLocationName = sourceLocation?.Name ?? string.Empty;

        var outputDescription = localizationService.GetLocalizedString("Movement.SupplyOutputDescription", supplyLocation.Name, woCode);
        var inputDescription = localizationService.GetLocalizedString("Movement.SupplyInputDescription", sourceLocationName, woCode);

        // 5. Check if stock with same dimensions and lot already exists at supply location
        var existingDestinationStock = stockService.GetByDimensionsAndLot(
            supplyLocation.Id,
            sourceStock.ReferenceId,
            sourceStock.Width,
            sourceStock.Length,
            sourceStock.Height,
            sourceStock.Diameter,
            sourceStock.Thickness,
            sourceStock.LotId);

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
                // Merge into existing stock at supply location (mateix lot, ja garantit per GetByDimensionsAndLot)
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
                    LotId = sourceStock.LotId,
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
            LotId = sourceStock.LotId,
            MovementType = StockMovementType.OUTPUT,
            Quantity = request.Quantity * -1,
            Width = sourceStock.Width,
            Length = sourceStock.Length,
            Height = sourceStock.Height,
            Diameter = sourceStock.Diameter,
            Thickness = sourceStock.Thickness,
            MovementDate = DateTime.Now,
            Description = outputDescription,
            Entity = StockMovementEntities.WorkOrderPhase,
            EntityId = request.WorkOrderPhaseId
        };

        // 7. INPUT movement record (history) - positive quantity at supply location
        var inputMovement = new StockMovement
        {
            StockId = destinationStockId,
            LocationId = supplyLocation.Id,
            ReferenceId = sourceStock.ReferenceId,
            LotId = sourceStock.LotId,
            MovementType = StockMovementType.INPUT,
            Quantity = request.Quantity,
            Width = sourceStock.Width,
            Length = sourceStock.Length,
            Height = sourceStock.Height,
            Diameter = sourceStock.Diameter,
            Thickness = sourceStock.Thickness,
            MovementDate = DateTime.Now,
            Description = inputDescription,
            Entity = StockMovementEntities.WorkOrderPhase,
            EntityId = request.WorkOrderPhaseId
        };

        await unitOfWork.StockMovements.AddRange([outputMovement, inputMovement]);

        if (sourceStock.LotId.HasValue)
            await UpdateLotRemainingQuantityAsync(sourceStock.LotId.Value);

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
        var isFullReturn = request.Quantity == sourceStock.Quantity;

        // Resolve WO code and supply location name for descriptions
        var woCode = await GetWorkOrderCodeByPhaseId(request.WorkOrderPhaseId);
        var supplyLocation = await unitOfWork.Warehouses.Locations.Get(supplyLocationId);
        var supplyLocationName = supplyLocation?.Name ?? string.Empty;

        var outputDescription = localizationService.GetLocalizedString("Movement.ReturnFromSupplyOutputDescription", supplyLocationName, woCode);
        var inputDescription = localizationService.GetLocalizedString("Movement.ReturnFromSupplyInputDescription", defaultLocation.Name, woCode);

        // 6. Check if stock with same dimensions and lot already exists at default location
        var existingDestinationStock = stockService.GetByDimensionsAndLot(
            defaultLocation.Id,
            sourceStock.ReferenceId,
            sourceStock.Width,
            sourceStock.Length,
            sourceStock.Height,
            sourceStock.Diameter,
            sourceStock.Thickness,
            sourceStock.LotId);

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
                    LotId = sourceStock.LotId,
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
            LotId = sourceStock.LotId,
            MovementType = StockMovementType.OUTPUT,
            Quantity = request.Quantity * -1,
            Width = sourceStock.Width,
            Length = sourceStock.Length,
            Height = sourceStock.Height,
            Diameter = sourceStock.Diameter,
            Thickness = sourceStock.Thickness,
            MovementDate = DateTime.Now,
            Description = outputDescription,
            Entity = StockMovementEntities.WorkOrderPhase,
            EntityId = request.WorkOrderPhaseId
        };

        // 8. INPUT movement record (history) - positive quantity at default location
        var inputMovement = new StockMovement
        {
            StockId = destinationStockId,
            LocationId = defaultLocation.Id,
            ReferenceId = sourceStock.ReferenceId,
            LotId = sourceStock.LotId,
            MovementType = StockMovementType.INPUT,
            Quantity = request.Quantity,
            Width = sourceStock.Width,
            Length = sourceStock.Length,
            Height = sourceStock.Height,
            Diameter = sourceStock.Diameter,
            Thickness = sourceStock.Thickness,
            MovementDate = DateTime.Now,
            Description = inputDescription,
            Entity = StockMovementEntities.WorkOrderPhase,
            EntityId = request.WorkOrderPhaseId
        };

        await unitOfWork.StockMovements.AddRange([outputMovement, inputMovement]);

        if (sourceStock.LotId.HasValue)
            await UpdateLotRemainingQuantityAsync(sourceStock.LotId.Value);

        return new GenericResponse(true);
    }

    public async Task<GenericResponse> ConsumePhaseStock(ConsumePhaseStockRequest request)
    {
        // 1. Validate request
        if (request.Entries == null || request.Entries.Count == 0)
            return new GenericResponse(false, localizationService.GetLocalizedString("Movement.Consumption.NoItems"));

        // 2. Get workcenter supply location IDs
        var workcenterLocations = await workcenterLocationService.GetByWorkcenterId(request.WorkcenterId);
        var workcenterLocationIds = workcenterLocations.Select(wl => wl.LocationId).ToHashSet();

        if (workcenterLocationIds.Count == 0)
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterSupplyLocationNotFound"));

        // 3. Get default warehouse location for returning remaining pieces
        var defaultLocationId = await warehouseService.GetDefaultLocation();
        if (defaultLocationId == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockDefaultLocationNotFound"));

        var defaultLocation = await unitOfWork.Warehouses.Locations.Get(defaultLocationId.Value);
        if (defaultLocation == null || defaultLocation.Disabled)
            return new GenericResponse(false, localizationService.GetLocalizedString("StockDefaultLocationNotFound"));

        // Resolve WO code for descriptions
        var woCode = await GetWorkOrderCodeByPhaseId(request.WorkOrderPhaseId);
        var consumptionDescription = localizationService.GetLocalizedString("Movement.ConsumptionDescription", woCode);
        var residueReturnDescription = localizationService.GetLocalizedString("Movement.ResidueReturnDescription", defaultLocation.Name, woCode);

        var allMovements = new List<StockMovement>();
        var touchedLotIds = new HashSet<Guid>();

        // 4. Process each stock entry
        foreach (var entry in request.Entries)
        {
            var sourceStock = await unitOfWork.Stocks.Get(entry.StockId);
            if (sourceStock == null)
                return new GenericResponse(false, localizationService.GetLocalizedString("StockNotFound"));

            // Validate stock is at a workcenter supply location
            if (!workcenterLocationIds.Contains(sourceStock.LocationId))
                return new GenericResponse(false, localizationService.GetLocalizedString("StockNotAtSupplyLocation"));

            var sourceLocationId = sourceStock.LocationId;
            var sourceQuantity = sourceStock.Quantity;

            if (sourceStock.LotId.HasValue)
                touchedLotIds.Add(sourceStock.LotId.Value);

            // 4a. Always consume the FULL source stock quantity.
            //     The entire provisioned material enters the production process.
            //     Remaining pieces (if any) will be returned as new/residue stock.
            sourceStock.Quantity = 0;
            sourceStock.LocationId = defaultLocation.Id;
            await unitOfWork.Stocks.Update(sourceStock);

            allMovements.Add(new StockMovement
            {
                StockId = sourceStock.Id,
                LocationId = sourceLocationId,
                ReferenceId = sourceStock.ReferenceId,
                LotId = sourceStock.LotId,
                MovementType = StockMovementType.CONSUMPTION,
                Quantity = sourceQuantity * -1,
                Width = sourceStock.Width,
                Length = sourceStock.Length,
                Height = sourceStock.Height,
                Diameter = sourceStock.Diameter,
                Thickness = sourceStock.Thickness,
                MovementDate = DateTime.Now,
                Description = consumptionDescription,
                Entity = StockMovementEntities.WorkOrderPhase,
                EntityId = request.WorkOrderPhaseId
            });

            // 4b. Return each remaining piece to the default location
            if (entry.RemainingPieces != null)
            {
                foreach (var piece in entry.RemainingPieces)
                {
                    if (piece.Quantity <= 0) continue;

                    // Find or create stock at default location with the piece's dimensions and same lot as the source
                    var existingStock = stockService.GetByDimensionsAndLot(
                        defaultLocation.Id,
                        sourceStock.ReferenceId,
                        piece.Width,
                        piece.Length,
                        piece.Height,
                        piece.Diameter,
                        piece.Thickness,
                        sourceStock.LotId);

                    Guid destinationStockId;

                    if (existingStock != null)
                    {
                        existingStock.Quantity += piece.Quantity;
                        await unitOfWork.Stocks.Update(existingStock);
                        destinationStockId = existingStock.Id;
                    }
                    else
                    {
                        var newStock = new Stock
                        {
                            ReferenceId = sourceStock.ReferenceId,
                            LocationId = defaultLocation.Id,
                            LotId = sourceStock.LotId,
                            Quantity = piece.Quantity,
                            Width = piece.Width,
                            Length = piece.Length,
                            Height = piece.Height,
                            Diameter = piece.Diameter,
                            Thickness = piece.Thickness
                        };
                        await unitOfWork.Stocks.Add(newStock);
                        destinationStockId = newStock.Id;
                    }

                    // CONSUMPTION movement for remaining piece (positive = returned material)
                    allMovements.Add(new StockMovement
                    {
                        StockId = destinationStockId,
                        LocationId = defaultLocation.Id,
                        ReferenceId = sourceStock.ReferenceId,
                        LotId = sourceStock.LotId,
                        MovementType = StockMovementType.CONSUMPTION,
                        Quantity = piece.Quantity,
                        Width = piece.Width,
                        Length = piece.Length,
                        Height = piece.Height,
                        Diameter = piece.Diameter,
                        Thickness = piece.Thickness,
                        MovementDate = DateTime.Now,
                        Description = residueReturnDescription,
                        Entity = StockMovementEntities.WorkOrderPhase,
                        EntityId = request.WorkOrderPhaseId
                    });
                }
            }
        }

        // 5. Auto-return: find remaining stock at workcenter supply locations
        //    scoped to the BOM references of the finalized phase only.
        //    This catches any provisioned stock NOT mentioned in request.Entries.
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
            var returnSourceLocationId = remainingStock.LocationId;
            var returnQuantity = remainingStock.Quantity;

            // Resolve supply location name for auto-return descriptions
            var returnSupplyLocation = await unitOfWork.Warehouses.Locations.Get(returnSourceLocationId);
            var returnSupplyLocationName = returnSupplyLocation?.Name ?? string.Empty;

            var autoReturnOutputDescription = localizationService.GetLocalizedString("Movement.ReturnFromSupplyOutputDescription", returnSupplyLocationName, woCode);
            var autoReturnInputDescription = localizationService.GetLocalizedString("Movement.ReturnFromSupplyInputDescription", defaultLocation.Name, woCode);

            if (remainingStock.LotId.HasValue)
                touchedLotIds.Add(remainingStock.LotId.Value);

            var existingDestinationStock = stockService.GetByDimensionsAndLot(
                defaultLocation.Id,
                remainingStock.ReferenceId,
                remainingStock.Width,
                remainingStock.Length,
                remainingStock.Height,
                remainingStock.Diameter,
                remainingStock.Thickness,
                remainingStock.LotId);

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
                // Merge into existing stock at default location (mateix lot, ja garantit per GetByDimensionsAndLot)
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
                LotId = remainingStock.LotId,
                MovementType = StockMovementType.OUTPUT,
                Quantity = returnQuantity * -1,
                Width = remainingStock.Width,
                Length = remainingStock.Length,
                Height = remainingStock.Height,
                Diameter = remainingStock.Diameter,
                Thickness = remainingStock.Thickness,
                MovementDate = DateTime.Now,
                Description = autoReturnOutputDescription,
                Entity = StockMovementEntities.WorkOrderPhase,
                EntityId = request.WorkOrderPhaseId
            });

            // INPUT movement (arriving at default location)
            allMovements.Add(new StockMovement
            {
                StockId = returnDestinationStockId,
                LocationId = defaultLocation.Id,
                ReferenceId = remainingStock.ReferenceId,
                LotId = remainingStock.LotId,
                MovementType = StockMovementType.INPUT,
                Quantity = returnQuantity,
                Width = remainingStock.Width,
                Length = remainingStock.Length,
                Height = remainingStock.Height,
                Diameter = remainingStock.Diameter,
                Thickness = remainingStock.Thickness,
                MovementDate = DateTime.Now,
                Description = autoReturnInputDescription,
                Entity = StockMovementEntities.WorkOrderPhase,
                EntityId = request.WorkOrderPhaseId
            });
        }

        // 6. Save all movements
        if (allMovements.Count > 0)
        {
            await unitOfWork.StockMovements.AddRange(allMovements);
        }

        // 7. Recalculate Lot.RemainingQuantity for every lot touched by this consumption
        foreach (var lotId in touchedLotIds)
        {
            await UpdateLotRemainingQuantityAsync(lotId);
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

        // 4. Resol el lot produït (fallback per OF antigues): només si la referència és loteada
        if (workOrder.DefaultProducedLotId == null)
        {
            var reference = await unitOfWork.References.Get(workOrder.ReferenceId);
            if (reference is { RequiresLot: true })
            {
                var producedLotResponse = await lotService.ResolveOrCreateLot(workOrder.ReferenceId, string.Empty, null, null);
                var producedLot = producedLotResponse.Content as Lot;
                if (producedLot != null)
                {
                    workOrder.DefaultProducedLotId = producedLot.Id;
                    await unitOfWork.WorkOrders.Update(workOrder);
                }
            }
        }

        // 5. Build the PRODUCTION stock movement
        var stockMovement = new StockMovement
        {
            ReferenceId = workOrder.ReferenceId,
            LocationId = defaultLocationId,
            LotId = workOrder.DefaultProducedLotId,
            MovementType = StockMovementType.PRODUCTION,
            Quantity = request.Quantity,
            MovementDate = DateTime.Now,
            Description = localizationService.GetLocalizedString("Movement.ProductionDescription", workOrder.Code),
            Entity = StockMovementEntities.WorkOrder,
            EntityId = workOrder.Id
        };

        // 6. Create stock movement (creates/updates Stock record and records the movement)
        return await stockMovementService.CreateProductionMovement(stockMovement);
    }

    // Recalcula Lot.RemainingQuantity sumant tot l'estoc del lot a totes les ubicacions; el tanca si arriba a 0 (mai el reobre).
    private async Task UpdateLotRemainingQuantityAsync(Guid lotId)
    {
        var lot = await unitOfWork.Lots.Get(lotId);
        if (lot == null) return;

        var lotStocks = await unitOfWork.Stocks.FindAsync(s => s.LotId == lotId);
        var total = lotStocks.Sum(s => s.Quantity);

        lot.RemainingQuantity = total;
        if (total == 0 && lot.ClosedDate == null)
            lot.ClosedDate = DateTime.Now;

        await unitOfWork.Lots.Update(lot);
    }
}
