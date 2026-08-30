using Application.Contracts;
using Domain.Entities.Shared;
using Domain.Entities.Warehouse;
using Microsoft.Extensions.Logging;

namespace Application.Services.Warehouse
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<StockMovementService> _logger;
        private readonly Guid? _defaultLocationId;

        public StockMovementService(
            IUnitOfWork unitOfWork,
            ILocalizationService localizationService,
            ILogger<StockMovementService> logger)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
            _logger = logger;

            var warehouse = _unitOfWork.Warehouses.Find(w => w.Disabled == false).FirstOrDefault();
            if (warehouse != null) _defaultLocationId = warehouse.DefaultLocationId;
        }

        public IEnumerable<StockMovement> GetBetweenDates(DateTime startDate, DateTime endDate, Guid? locationId)
        {
            return _unitOfWork.StockMovements.GetBetweenDatesWithLocation(startDate, endDate, locationId);
        }

        public async Task<GenericResponse> Create(StockMovement request)
        {
            if (_defaultLocationId == null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("StockDefaultLocationNotDefined"));

            var movementLocation = _defaultLocationId.Value;
            if (request.LocationId != null)
            {
                movementLocation = request.LocationId.Value;
            }

            // Comprovar si la referència es un servei. Si es un servei no es genera moviment ni error.
            var reference = _unitOfWork.References.Find(p => p.Id == request.ReferenceId).FirstOrDefault();
            if (reference == null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("ReferenceNotExistent", request.ReferenceId));

            if (reference.CategoryName == ReferenceCategories.Service)
            {
                return new GenericResponse(true, request);
            }

            // Comprovar si existeix un stock id per les dimensions, producte i lot
            var stock = GetByDimensions(movementLocation, request.ReferenceId,
                                        request.Width, request.Length, request.Height,
                                        request.Diameter, request.Thickness, request.LotId);

            if (request.MovementType == StockMovementType.OUTPUT)
            {
                if (request.Quantity > 0) request.Quantity *= -1;
            }
            else if (request.MovementType == StockMovementType.INPUT)
            {
                if (request.Quantity < 0) request.Quantity *= -1;
            }

            // Un lot tancat no pot tornar a rebre stock
            if (request.LotId.HasValue && request.Quantity > 0)
            {
                var lot = await _unitOfWork.Lots.Get(request.LotId.Value);
                if (lot != null && lot.ClosedDate != null)
                    return new GenericResponse(false, _localizationService.GetLocalizedString("LotClosed"));
            }

            if (stock != null)
            {
                stock.LocationId = movementLocation;
                stock.Quantity += request.Quantity;
                await _unitOfWork.Stocks.Update(stock);

                request.StockId = stock.Id;
            }
            else
            {
                var newStock = new Stock
                {
                    ReferenceId = request.ReferenceId,
                    LocationId = movementLocation,
                    LotId = request.LotId,
                    Quantity = request.Quantity,
                    Width = request.Width,
                    Length = request.Length,
                    Height = request.Height,
                    Diameter = request.Diameter,
                    Thickness = request.Thickness
                };
                await _unitOfWork.Stocks.Add(newStock);

                request.StockId = newStock.Id;
            }

            if (request.LotId.HasValue)
                await UpdateLotRemainingQuantityAsync(request.LotId.Value);

            request.LocationId = movementLocation;
            await _unitOfWork.StockMovements.Add(request);
            return new GenericResponse(true, request);
        }

        /// <inheritdoc cref="IStockMovementService.ApplyDeliveryNoteStockBatch"/>
        public async Task<GenericResponse> ApplyDeliveryNoteStockBatch(IEnumerable<StockMovement> movements)
        {
            if (_defaultLocationId == null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("StockDefaultLocationNotDefined"));

            var movementList = movements.ToList();
            if (movementList.Count == 0)
                return new GenericResponse(true);

            // --- 1. Carregar totes les referències en una consulta ---
            var referenceIds = movementList.Select(m => m.ReferenceId).Distinct().ToList();
            var references = _unitOfWork.References
                .Find(r => referenceIds.Contains(r.Id))
                .ToDictionary(r => r.Id);

            // --- 2. Validar existència; filtrar serveis ---
            var nonServiceMovements = new List<StockMovement>();
            foreach (var mov in movementList)
            {
                if (!references.TryGetValue(mov.ReferenceId, out var reference))
                    return new GenericResponse(false,
                        _localizationService.GetLocalizedString("ReferenceNotExistent", mov.ReferenceId));

                if (reference.CategoryName == ReferenceCategories.Service)
                    continue;   // igual que Create: sense error ni moviment

                // Normalitzar signe
                if (mov.MovementType == StockMovementType.OUTPUT)
                {
                    if (mov.Quantity > 0) mov.Quantity *= -1;
                }
                else if (mov.MovementType == StockMovementType.INPUT)
                {
                    if (mov.Quantity < 0) mov.Quantity *= -1;
                }

                mov.LocationId ??= _defaultLocationId.Value;
                nonServiceMovements.Add(mov);
            }

            if (nonServiceMovements.Count == 0)
            {
                return new GenericResponse(true);
            }

            var lotIds = nonServiceMovements
                .Where(m => m.LotId.HasValue)
                .Select(m => m.LotId!.Value)
                .Distinct()
                .ToList();
            var lotsById = lotIds.Count == 0
                ? new Dictionary<Guid, Lot>()
                : _unitOfWork.Lots
                    .Find(lot => lotIds.Contains(lot.Id))
                    .ToDictionary(lot => lot.Id);
            var closedLotMovement = nonServiceMovements.FirstOrDefault(m =>
                m.Quantity > 0 &&
                m.LotId.HasValue &&
                lotsById.TryGetValue(m.LotId.Value, out var lot) &&
                lot.ClosedDate.HasValue);
            if (closedLotMovement != null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("LotClosed"));

            var locationIds = nonServiceMovements.Select(m => m.LocationId!.Value).Distinct().ToList();
            var movRefIds = nonServiceMovements.Select(m => m.ReferenceId).Distinct().ToList();

            var existingStocks = _unitOfWork.Stocks
                .Find(s =>
                    (locationIds.Contains(s.LocationId) && movRefIds.Contains(s.ReferenceId)) ||
                    (s.LotId.HasValue && lotIds.Contains(s.LotId.Value)))
                .ToList();

            var stockByKey = existingStocks
                .GroupBy(s =>
                    (s.LocationId, s.ReferenceId, s.LotId, s.Width, s.Length, s.Height, s.Diameter, s.Thickness))
                .ToDictionary(group => group.Key, group => group.First());

            // --- 4. Acumular moviments per clau (pot haver referència repetida al mateix albarà) ---
            // Primer agrupem els moviments per clau dimensional per acumular quantitats
            var grouped = nonServiceMovements
                .GroupBy(m => (
                    LocationId: m.LocationId!.Value,
                    m.ReferenceId,
                    m.LotId,
                    m.Width, m.Length, m.Height, m.Diameter, m.Thickness))
                .ToList();

            var newStocksByKey = new Dictionary<(Guid, Guid, Guid?, decimal, decimal, decimal, decimal, decimal), Stock>();

            foreach (var group in grouped)
            {
                var key = group.Key;
                var totalQty = group.Sum(m => m.Quantity);

                if (stockByKey.TryGetValue(key, out var existing))
                {
                    existing.Quantity += totalQty;
                    _unitOfWork.Stocks.UpdateWithoutSave(existing);
                    foreach (var m in group) m.StockId = existing.Id;
                }
                else if (newStocksByKey.TryGetValue(key, out var pending))
                {
                    // Segon o posterior grup per la mateixa clau (no hauria de passar en batch
                    // del mateix albarà, però per robustesa):
                    pending.Quantity += totalQty;
                    foreach (var m in group) m.StockId = pending.Id;
                }
                else
                {
                    var newStock = new Stock
                    {
                        ReferenceId = key.ReferenceId,
                        LocationId = key.LocationId,
                        LotId = key.LotId,
                        Quantity = totalQty,
                        Width = key.Width,
                        Length = key.Length,
                        Height = key.Height,
                        Diameter = key.Diameter,
                        Thickness = key.Thickness,
                    };
                    await _unitOfWork.Stocks.AddWithoutSave(newStock);
                    newStocksByKey[key] = newStock;
                    foreach (var m in group) m.StockId = newStock.Id;
                }
            }

            // --- 5. Afegir moviments sense Save individual ---
            foreach (var m in nonServiceMovements)
            {
                await _unitOfWork.StockMovements.AddWithoutSave(m);
            }

            var affectedStocks = existingStocks.Concat(newStocksByKey.Values).ToList();
            foreach (var lotId in lotIds)
            {
                if (!lotsById.TryGetValue(lotId, out var lot)) continue;

                var remainingQuantity = affectedStocks
                    .Where(stock => stock.LotId == lotId)
                    .Sum(stock => stock.Quantity);
                lot.RemainingQuantity = remainingQuantity;
                if (remainingQuantity == 0 && !lot.ClosedDate.HasValue)
                    lot.ClosedDate = DateTime.Now;

                _unitOfWork.Lots.UpdateWithoutSave(lot);
            }

            await _unitOfWork.CompleteAsync();

            return new GenericResponse(true);
        }


        public async Task<GenericResponse> CreateProductionMovement(StockMovement request)
        {
            if (request.LocationId == null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("StockDefaultLocationNotDefined"));

            // Un lot tancat no pot tornar a rebre stock
            if (request.LotId.HasValue)
            {
                var lot = await _unitOfWork.Lots.Get(request.LotId.Value);
                if (lot != null && lot.ClosedDate != null)
                    return new GenericResponse(false, _localizationService.GetLocalizedString("LotClosed"));
            }

            // Comprovar si existeix stock de la referencia i lot
            var stock = (await _unitOfWork.Stocks.FindAsync(s => s.ReferenceId == request.ReferenceId && s.LotId == request.LotId))
                           .FirstOrDefault();

            if (stock == null)
            {
                var newStock = new Stock
                {
                    ReferenceId = request.ReferenceId,
                    LocationId = request.LocationId.Value,
                    LotId = request.LotId,
                    Quantity = request.Quantity,
                    Width = request.Width,
                    Length = request.Length,
                    Height = request.Height,
                    Diameter = request.Diameter,
                    Thickness = request.Thickness
                };
                await _unitOfWork.Stocks.Add(newStock);

                request.StockId = newStock.Id;
            }
            else
            {
                stock.LocationId = request.LocationId.Value;
                stock.Quantity += request.Quantity;
                await _unitOfWork.Stocks.Update(stock);

                request.StockId = stock.Id;
            }

            if (request.LotId.HasValue)
                await UpdateLotRemainingQuantityAsync(request.LotId.Value);

            await _unitOfWork.StockMovements.Add(request);
            return new GenericResponse(true, request);
        }

        public async Task<IEnumerable<StockMovement>> GetByWorkOrderId(Guid workOrderId)
        {
            var workOrder = await _unitOfWork.WorkOrders.GetDetailed(workOrderId);
            if (workOrder == null)
                return Enumerable.Empty<StockMovement>();

            var phaseIds = workOrder.Phases.Select(p => p.Id).ToList();

            return _unitOfWork.StockMovements.GetByEntityReferences(workOrderId, phaseIds);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            if (_defaultLocationId == null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("StockDefaultLocationNotDefined"));

            var stockMovement = await _unitOfWork.StockMovements.Get(id);
            if (stockMovement == null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.IdNotExist", id));

            var stockLocationId = stockMovement.LocationId ?? _defaultLocationId.Value;
            var stock = GetByDimensions(stockLocationId, stockMovement.ReferenceId,
                                        stockMovement.Width, stockMovement.Length, stockMovement.Height,
                                        stockMovement.Diameter, stockMovement.Thickness, stockMovement.LotId);

            if (stock == null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("StockNotFound"));

            stock.Quantity += -1 * stockMovement.Quantity;
            await _unitOfWork.Stocks.Update(stock);

            if (stockMovement.LotId.HasValue)
                await UpdateLotRemainingQuantityAsync(stockMovement.LotId.Value);

            await _unitOfWork.StockMovements.Remove(stockMovement);
            return new GenericResponse(true);
        }

        private Stock? GetByDimensions(Guid locationId, Guid referenceId, decimal width, decimal length, decimal height, decimal diameter, decimal thickness, Guid? lotId)
        {
            return _unitOfWork.Stocks.Find(
                p => p.LocationId == locationId &&
                     p.ReferenceId == referenceId &&
                     p.Width == width &&
                     p.Length == length &&
                     p.Height == height &&
                     p.Diameter == diameter &&
                     p.Thickness == thickness &&
                     p.LotId == lotId
            ).FirstOrDefault();
        }

        // Recalcula Lot.RemainingQuantity sumant tot l'estoc del lot a totes les ubicacions; el tanca si arriba a 0 (mai el reobre).
        private async Task UpdateLotRemainingQuantityAsync(Guid lotId)
        {
            var lot = await _unitOfWork.Lots.Get(lotId);
            if (lot == null) return;

            var lotStocks = await _unitOfWork.Stocks.FindAsync(s => s.LotId == lotId);
            var total = lotStocks.Sum(s => s.Quantity);

            lot.RemainingQuantity = total;
            if (total == 0 && lot.ClosedDate == null)
                lot.ClosedDate = DateTime.Now;

            await _unitOfWork.Lots.Update(lot);
        }
    }
}





