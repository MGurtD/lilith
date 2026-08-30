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

            // Comprovar si existeix un stock id per les dimensions i producte
            var stock = GetByDimensions(movementLocation, request.ReferenceId,
                                        request.Width, request.Length, request.Height,
                                        request.Diameter, request.Thickness);

            if (request.MovementType == StockMovementType.OUTPUT)
            {
                if (request.Quantity > 0) request.Quantity *= -1;
            }
            else if (request.MovementType == StockMovementType.INPUT)
            {
                if (request.Quantity < 0) request.Quantity *= -1;
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

            // --- 3. Carregar tots els estocs afectats en una consulta ---
            // Construir clau composta per acumular quantitats de moviments repetits
            // Key = (LocationId, ReferenceId, Width, Length, Height, Diameter, Thickness)
            var locationIds = nonServiceMovements.Select(m => m.LocationId!.Value).Distinct().ToList();
            var movRefIds = nonServiceMovements.Select(m => m.ReferenceId).Distinct().ToList();

            var existingStocks = _unitOfWork.Stocks
                .Find(s => locationIds.Contains(s.LocationId) && movRefIds.Contains(s.ReferenceId))
                .ToList();

            // Diccionari de stocks per clau dimensional per evitar tracking duplicat
            var stockByKey = existingStocks
                .GroupBy(s =>
                    (s.LocationId, s.ReferenceId, s.Width, s.Length, s.Height, s.Diameter, s.Thickness))
                .ToDictionary(group => group.Key, group => group.First());

            // --- 4. Acumular moviments per clau (pot haver referència repetida al mateix albarà) ---
            // Primer agrupem els moviments per clau dimensional per acumular quantitats
            var grouped = nonServiceMovements
                .GroupBy(m => (
                    LocationId: m.LocationId!.Value,
                    m.ReferenceId,
                    m.Width, m.Length, m.Height, m.Diameter, m.Thickness))
                .ToList();

            // Stocks nous: guardats aquí per poder assignar StockId als moviments del grup
            var newStocksByKey = new Dictionary<(Guid, Guid, decimal, decimal, decimal, decimal, decimal), Stock>();

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

            // --- 6. Un únic CompleteAsync per a tot el batch ---
            await _unitOfWork.CompleteAsync();

            return new GenericResponse(true);
        }


        public async Task<GenericResponse> CreateProductionMovement(StockMovement request)
        {
            if (request.LocationId == null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("StockDefaultLocationNotDefined"));

            // Comprovar si existeix stock de la referencia
            var stock = (await _unitOfWork.Stocks.FindAsync(s => s.ReferenceId == request.ReferenceId))
                           .FirstOrDefault();

            if (stock == null)
            {
                var newStock = new Stock
                {
                    ReferenceId = request.ReferenceId,
                    LocationId = request.LocationId.Value,
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
                                        stockMovement.Diameter, stockMovement.Thickness);

            if (stock == null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("StockNotFound"));

            stock.Quantity += -1 * stockMovement.Quantity;
            await _unitOfWork.Stocks.Update(stock);

            await _unitOfWork.StockMovements.Remove(stockMovement);
            return new GenericResponse(true);
        }

        private Stock? GetByDimensions(Guid locationId, Guid referenceId, decimal width, decimal length, decimal height, decimal diameter, decimal thickness)
        {
            return _unitOfWork.Stocks.Find(
                p => p.LocationId == locationId &&
                     p.ReferenceId == referenceId &&
                     p.Width == width &&
                     p.Length == length &&
                     p.Height == height &&
                     p.Diameter == diameter &&
                     p.Thickness == thickness
            ).FirstOrDefault();
        }
    }
}





