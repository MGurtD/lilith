using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Contracts;

public interface IWarehouseService
{
    Task<Domain.Entities.Warehouse.Warehouse?> GetById(Guid id);
    Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetAll();
    Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetBySiteId(Guid siteId);
    Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetAllWithLocations();
    Task<GenericResponse> Create(Domain.Entities.Warehouse.Warehouse warehouse);
    Task<GenericResponse> Update(Domain.Entities.Warehouse.Warehouse warehouse);
    Task<GenericResponse> Remove(Guid id);

    // Location nested operations
    Task<GenericResponse> CreateLocation(Location location);
    Task<GenericResponse> UpdateLocation(Location location);
    Task<GenericResponse> RemoveLocation(Guid id);
}
