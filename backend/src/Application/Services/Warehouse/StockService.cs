using Application.Contracts;
using Domain.Entities.Warehouse;
using Domain.Implementations.ReferenceFormat;

#pragma warning disable IDE0060

namespace Application.Services.Warehouse
{
    public class StockService(IUnitOfWork unitOfWork) : IStockService
    {
        public async Task<GenericResponse> Create(Stock request)
        {
            await unitOfWork.Stocks.Add(request);
            return new GenericResponse(true, request);
        }

        public async Task<GenericResponse> Update(Stock request)
        {
            await unitOfWork.Stocks.Update(request);
            return new GenericResponse(true, request);
        }

        public async Task<GenericResponse> Delete(Stock request)
        {
            await unitOfWork.Stocks.Remove(request);
            return new GenericResponse(true, request);
        }

        public async Task<IEnumerable<StockListItemResponse>> GetAll(Guid? locationId, Guid? referenceId)
        {
            return await unitOfWork.Warehouses.GetStockList(locationId, referenceId);
        }

        public Stock? GetByDimensions(Guid locationId, Guid referenceId, decimal width, decimal length, decimal height, decimal diameter, decimal thickness)
        {
            var stocks = unitOfWork.Stocks.Find(
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

        public async Task<IEnumerable<StockResponse>> GetStockByWorkOrderPhaseBillOfMaterialsId(Guid id)
        {
            var bom = await unitOfWork.WorkOrders.Phases.BillOfMaterials.Get(id);
            if (bom == null) return [];

            var reference = await unitOfWork.References.Get(bom.ReferenceId);
            if (reference == null) return [];

            var stock = await unitOfWork.Warehouses.GetStockByReferenceId(bom.ReferenceId);

            // Filtrar stock segons el format de la referència
            if (reference.ReferenceFormatId.HasValue)
            {
                var format = await unitOfWork.ReferenceFormats.Get(reference.ReferenceFormatId.Value);
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






