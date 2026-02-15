using Application.Contracts;
using Application.Services;
using Domain.Entities.Warehouse;

namespace Application.Services.Warehouse;

public class WarehouseService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IWarehouseService
{
    public async Task<Domain.Entities.Warehouse.Warehouse?> GetById(Guid id)
    {
        return await unitOfWork.Warehouses.Get(id);
    }

    public async Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetAll()
    {
        return await unitOfWork.Warehouses.GetAll();
    }

    public async Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetBySiteId(Guid siteId)
    {
        return await unitOfWork.Warehouses.GetBySiteId(siteId);
    }

    public async Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetAllWithLocations()
    {
        return await unitOfWork.Warehouses.GetAllWithLocations();
    }

    public async Task<GenericResponse> Create(Domain.Entities.Warehouse.Warehouse warehouse)
    {
        var exists = unitOfWork.Warehouses.Find(w => w.Name == warehouse.Name).Any();
        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("WarehouseAlreadyExists", warehouse.Name));
        }

        await unitOfWork.Warehouses.Add(warehouse);
        return new GenericResponse(true, warehouse);
    }

    public async Task<GenericResponse> Update(Domain.Entities.Warehouse.Warehouse warehouse)
    {
        var exists = await unitOfWork.Warehouses.Exists(warehouse.Id);
        if (!exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", warehouse.Id));
        }

        await unitOfWork.Warehouses.Update(warehouse);
        return new GenericResponse(true, warehouse);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.Warehouses.Find(w => w.Id == id).FirstOrDefault();
        if (entity == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.Warehouses.Remove(entity);
        return new GenericResponse(true, entity);
    }

    // Location nested operations
    public async Task<GenericResponse> CreateLocation(Location location)
    {
        var exists = unitOfWork.Warehouses.Locations.Find(l =>
            l.Name == location.Name && l.WarehouseId == location.WarehouseId).Any();

        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("LocationAlreadyExists", location.Name));
        }

        await unitOfWork.Warehouses.Locations.Add(location);
        return new GenericResponse(true, location);
    }

    public async Task<GenericResponse> UpdateLocation(Location location)
    {
        var exists = unitOfWork.Warehouses.Locations.Find(l => l.Id == location.Id).Any();
        if (!exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("LocationNotFound", location.Id));
        }

        await unitOfWork.Warehouses.Locations.Update(location);
        return new GenericResponse(true, location);
    }

    public async Task<GenericResponse> RemoveLocation(Guid id)
    {
        var location = unitOfWork.Warehouses.Locations.Find(l => l.Id == id).FirstOrDefault();
        if (location == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("LocationNotFound", id));
        }

        await unitOfWork.Warehouses.Locations.Remove(location);
        return new GenericResponse(true, location);
    }
}
