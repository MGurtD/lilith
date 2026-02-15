using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Contracts
{
    public interface IStockService
    {
        Task<GenericResponse> Create(Stock request);
        Task<GenericResponse> Update(Stock request);
        Task<GenericResponse> Delete(Stock request);
        IEnumerable<Stock> GetByLocation(Guid locationId);
        IEnumerable<Stock> GetByReference(Guid referenceId);
        Stock? GetByDimensions(Guid locationId, Guid referenceId, decimal width, decimal length, decimal height, decimal diameter, decimal thickness);
        IEnumerable<Stock> GetAll();

    }
}
