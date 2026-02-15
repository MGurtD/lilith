using Application.Contracts;
using Domain.Entities.Warehouse;

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
    }
}





