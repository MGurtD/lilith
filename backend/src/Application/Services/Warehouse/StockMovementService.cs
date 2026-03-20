using Application.Contracts;
using Domain.Entities.Shared;
using Domain.Entities.Warehouse;

namespace Application.Services.Warehouse
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;
        private readonly Guid? _defaultLocationId;

        public StockMovementService(IUnitOfWork unitOfWork, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;

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

            if (reference.CategoryName == ReferenceCategories.Service) return new GenericResponse(true, request);

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





