using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Contracts
{
    public interface IStockService
    {
        Task<GenericResponse> Create(Stock request);
        Task<GenericResponse> Update(Stock request);
        Task<GenericResponse> Delete(Stock request);
        Task<IEnumerable<StockListItemResponse>> GetAll(Guid? locationId, Guid? referenceId);
        Stock? GetByDimensionsAndLot(Guid locationId, Guid referenceId, decimal width, decimal length, decimal height, decimal diameter, decimal thickness, Guid? lotId);
        Task<IEnumerable<StockResponse>> GetStockByWorkOrderPhaseBillOfMaterialsId(Guid id);
    }
}
