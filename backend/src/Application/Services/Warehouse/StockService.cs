using Application.Contracts;
using Domain.Constants;
using Domain.Entities.Warehouse;
using Domain.Implementations.ReferenceFormat;

namespace Application.Services.Warehouse
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkcenterLocationService _workcenterLocationService;

        public StockService(IUnitOfWork unitOfWork, ILocalizationService localizationService, IWorkcenterLocationService workcenterLocationService)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
            _workcenterLocationService = workcenterLocationService;
        }
        public async Task<GenericResponse> Create(Stock request)
        {
            await _unitOfWork.Stocks.Add(request);
            return new GenericResponse(true, request);
        }

        public async Task<GenericResponse> Update(Stock request)
        {
            await _unitOfWork.Stocks.Update(request);
            return new GenericResponse(true, request);
        }

        public async Task<GenericResponse> Delete(Stock request)
        {
            await _unitOfWork.Stocks.Remove(request);
            return new GenericResponse(true, request);
        }

        public IEnumerable<Stock> GetByLocation(Guid locationId)
        {
            var stocks = _unitOfWork.Stocks.Find(p => p.LocationId == locationId)
                            .GroupBy(p => new { p.Width, p.Length, p.Height, p.Diameter, p.Thickness })
                            .Select(b => new Stock
                            {
                                LocationId = locationId,
                                Width = b.Key.Width,
                                Length = b.Key.Length,
                                Height = b.Key.Height,
                                Diameter = b.Key.Diameter,
                                Thickness = b.Key.Thickness,
                                Quantity = b.Sum(x => x.Quantity)
                            });
            return stocks;
        }

        public IEnumerable<Stock> GetByReference(Guid referenceId)
        {
            var stocks = _unitOfWork.Stocks.Find(p => p.ReferenceId == referenceId)
                            .GroupBy(p => new { p.Width, p.Length, p.Height, p.Diameter, p.Thickness })
                            .Select(b => new Stock
                            {
                                ReferenceId = referenceId,
                                Width = b.Key.Width,
                                Length = b.Key.Length,
                                Height = b.Key.Height,
                                Diameter = b.Key.Diameter,
                                Thickness = b.Key.Thickness,
                                Quantity = b.Sum(x => x.Quantity)
                            });
            return stocks;
        }

    public Stock? GetByDimensions(Guid locationId, Guid referenceId, decimal width, decimal length, decimal height, decimal diameter, decimal thickness)
        {
            var stocks = _unitOfWork.Stocks.Find(
                p => p.LocationId == locationId &&
                    p.ReferenceId == referenceId &&
                    p.Width == width &&
                    p.Length == length &&
                    p.Height == height &&
                    p.Diameter == diameter &&
                    p.Thickness == thickness
            ).FirstOrDefault();
            return stocks;
        }

        public IEnumerable<Stock> GetAll()
        {
            var stock = _unitOfWork.Stocks.Find(p => p.Quantity > 0);
            return stock;
        }
        public async Task<IEnumerable<StockResponse>> GetStockByWorkOrderPhaseBillOfMaterialsId(Guid id)
        {
            var bom = await _unitOfWork.WorkOrders.Phases.BillOfMaterials.Get(id);
            if (bom == null) return Enumerable.Empty<StockResponse>();

            var reference = await _unitOfWork.References.Get(bom.ReferenceId);
            if (reference == null) return Enumerable.Empty<StockResponse>();

            var stock = await _unitOfWork.Warehouses.GetStockByReferenceId(bom.ReferenceId);

            // Filtrar stock segons el format de la referència
            if (reference.ReferenceFormatId.HasValue)
            {
                var format = await _unitOfWork.ReferenceFormats.Get(reference.ReferenceFormatId.Value);
                if (format != null)
                {
                    var stockFilter = ReferenceFormatCalculationFactory.CreateStockFilter(format.Code);
                    stock = stock.Where(s => stockFilter.IsCompatible(
                        s.Width, s.Length, s.Height, s.Diameter, s.Thickness,
                        bom.Width, bom.Length, bom.Height, bom.Diameter, bom.Thickness));

                    // Ordenar per merma ascendent (la proposta amb menys merma primer)
                    stock = stock.OrderBy(s => stockFilter.CalculateWaste(
                        s.Width, s.Length, s.Height, s.Diameter, s.Thickness,
                        bom.Width, bom.Length, bom.Height, bom.Diameter, bom.Thickness,
                        bom.Quantity) ?? decimal.MaxValue);
                }
            }

            return stock;
        }

        public async Task<GenericResponse> MoveToWorkcenterSupply(MoveStockToWorkcenterSupplyRequest request)
        {
            // 1. Validate and get source stock
            var sourceStock = await _unitOfWork.Stocks.Get(request.StockId);
            if (sourceStock == null)
                return new GenericResponse(false, _localizationService.GetLocalizedString("StockNotFound"));

            if (sourceStock.Quantity < request.Quantity)
                return new GenericResponse(false, _localizationService.GetLocalizedString("StockInsufficientQuantity"));

            // 2. Find the Supply location for the workcenter
            var workcenterLocations = await _workcenterLocationService.GetByWorkcenterId(request.WorkcenterId);
            var workcenterLocationsList = workcenterLocations.ToList();

            if (!workcenterLocationsList.Any())
                return new GenericResponse(false, _localizationService.GetLocalizedString("WorkcenterSupplyLocationNotFound"));

            var supplyLocationId = workcenterLocationsList.First().LocationId;
            var supplyLocation = await _unitOfWork.Warehouses.Locations.Get(supplyLocationId);

            if (supplyLocation == null || supplyLocation.Disabled)
                return new GenericResponse(false, _localizationService.GetLocalizedString("WorkcenterSupplyLocationNotFound"));

            // 3. If the stock is already at the supply location, nothing to do
            if (sourceStock.LocationId == supplyLocation.Id)
                return new GenericResponse(true, sourceStock);

            // Remember source location name for the movement description
            var sourceLocationName = sourceStock.Location?.Name ?? "warehouse";

            // 3. Update stock location
            Stock destinationStock;
            if (sourceStock.Quantity == request.Quantity)
            {
                // Full quantity: relocate the stock record to the supply location
                sourceStock.LocationId = supplyLocation.Id;
                await _unitOfWork.Stocks.Update(sourceStock);
                destinationStock = sourceStock;
            }
            else
            {
                // Partial quantity: decrease source, find-or-create at destination
                sourceStock.Quantity -= request.Quantity;
                await _unitOfWork.Stocks.Update(sourceStock);

                var existingDestination = GetByDimensions(
                    supplyLocation.Id,
                    sourceStock.ReferenceId,
                    sourceStock.Width,
                    sourceStock.Length,
                    sourceStock.Height,
                    sourceStock.Diameter,
                    sourceStock.Thickness
                );

                if (existingDestination != null)
                {
                    existingDestination.Quantity += request.Quantity;
                    await _unitOfWork.Stocks.Update(existingDestination);
                    destinationStock = existingDestination;
                }
                else
                {
                    destinationStock = new Stock
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
                    await _unitOfWork.Stocks.Add(destinationStock);
                }
            }

            // 4. Create a single SUPPLY movement for traceability
            var supplyMovement = new StockMovement
            {
                StockId = destinationStock.Id,
                LocationId = supplyLocation.Id,
                ReferenceId = destinationStock.ReferenceId,
                MovementType = StockMovementType.SUPPLY,
                Quantity = request.Quantity,
                Width = destinationStock.Width,
                Length = destinationStock.Length,
                Height = destinationStock.Height,
                Diameter = destinationStock.Diameter,
                Thickness = destinationStock.Thickness,
                MovementDate = DateTime.UtcNow,
                Description = _localizationService.GetLocalizedString("Movement.TransferToSupplyDescription", sourceLocationName, supplyLocation.Name)
            };
            await _unitOfWork.StockMovements.Add(supplyMovement);

            return new GenericResponse(true, destinationStock);
        }
    }
}






