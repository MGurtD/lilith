using Application.Contracts;
using Domain.Entities.Warehouse;
using Domain.Implementations.ReferenceFormat;

namespace Application.Services.Warehouse
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StockService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    }
}






