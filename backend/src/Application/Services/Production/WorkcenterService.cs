using Application.Contracts;
using Application.Services;
using Domain.Constants;
using Domain.Entities.Production;
using Domain.Entities.Warehouse;

namespace Application.Services.Production;

public class WorkcenterService(IUnitOfWork unitOfWork, ILocalizationService localizationService, IWarehouseService warehouseService) : IWorkcenterService
{
    public async Task<Workcenter?> GetById(Guid id)
    {
        return await unitOfWork.Workcenters.Get(id);
    }

    public async Task<IEnumerable<Workcenter>> GetAll()
    {
        var entities = await unitOfWork.Workcenters.GetAll();
        return entities.OrderBy(w => w.Name);
    }

    public async Task<IEnumerable<Workcenter>> GetVisibleInPlant()
    {
        return await unitOfWork.Workcenters.GetVisibleInPlant();
    }

    public async Task<IEnumerable<WorkcenterLoadDto>> GetWorkcenterLoadBetweenDatesByWorkcenterType(DateTime startDate, DateTime endDate)
    {
        return await unitOfWork.Workcenters.GetWorkcenterLoadBetweenDatesByWorkcenterType(startDate, endDate);
    }

    public async Task<GenericResponse> Create(Workcenter workcenter)
    {
        var exists = unitOfWork.Workcenters.Find(w => w.Name == workcenter.Name).Any();
        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("WorkcenterAlreadyExists", workcenter.Name));
        }

        await unitOfWork.Workcenters.Add(workcenter);

        // Auto-create supply location and link via WorkcenterLocations
        var supplyLocation = await CreateSupplyLocation(workcenter.Name);
        if (supplyLocation is not null)
        {
            var link = new WorkcenterLocation
            {
                Id = Guid.NewGuid(),
                WorkcenterId = workcenter.Id,
                LocationId = supplyLocation.Id
            };
            await unitOfWork.WorkcenterLocations.Add(link);
        }

        return new GenericResponse(true, workcenter);
    }

    public async Task<GenericResponse> Update(Workcenter workcenter)
    {
        var existing = unitOfWork.Workcenters.Find(w => w.Id == workcenter.Id).FirstOrDefault();
        if (existing is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", workcenter.Id));
        }

        // Sync supply location(s) disabled state when workcenter disabled state changes
        if (existing.Disabled != workcenter.Disabled)
        {
            var links = unitOfWork.WorkcenterLocations
                .Find(wl => wl.WorkcenterId == workcenter.Id)
                .ToList();

            foreach (var link in links)
            {
                var location = unitOfWork.Warehouses.Locations
                    .Find(l => l.Id == link.LocationId && l.LocationType == LocationTypeConstants.Supply)
                    .FirstOrDefault();

                if (location is not null)
                {
                    location.Disabled = workcenter.Disabled;
                    await warehouseService.UpdateLocation(location);
                }
            }
        }

        await unitOfWork.Workcenters.Update(workcenter);
        return new GenericResponse(true, workcenter);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.Workcenters.Find(w => w.Id == id).FirstOrDefault();
        if (entity == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        var supplyLocationIds = unitOfWork.WorkcenterLocations
            .Find(wl => wl.WorkcenterId == id)
            .Select(wl => wl.LocationId)
            .ToList()
            .Where(locationId => unitOfWork.Warehouses.Locations
                .Find(l => l.Id == locationId && l.LocationType == LocationTypeConstants.Supply)
                .Any())
            .ToList();

        // The workcenter goes first, so a workcenter in use is refused before anything
        // else changes. Its location links cascade from it; removing them by hand after
        // their location failed with a concurrency error (#159).
        await unitOfWork.Workcenters.Remove(entity);

        foreach (var locationId in supplyLocationIds)
        {
            try
            {
                await warehouseService.RemoveLocation(locationId);
            }
            catch (EntityInUseException)
            {
                // A supply location that still holds stock stays in its warehouse.
            }
        }

        return new GenericResponse(true, entity);
    }

    public async Task<IEnumerable<Location>> GetLocationsByType(Guid workcenterId, string locationType)
    {
        var locationIds = unitOfWork.WorkcenterLocations
            .Find(wl => wl.WorkcenterId == workcenterId)
            .Select(wl => wl.LocationId)
            .ToHashSet();

        var locations = unitOfWork.Warehouses.Locations
            .Find(l => locationIds.Contains(l.Id) &&
                       l.LocationType == locationType &&
                       !l.Disabled)
            .ToList();

        return await Task.FromResult(locations);
    }

    private async Task<Location?> CreateSupplyLocation(string workcenterName)
    {
        var warehouse = unitOfWork.Warehouses.Find(w => !w.Disabled).FirstOrDefault();
        if (warehouse is null) return null;

        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = $"APR-{workcenterName}",
            Description = $"Ubicació d'aprovisionament per {workcenterName}",
            WarehouseId = warehouse.Id,
            LocationType = LocationTypeConstants.Supply
        };

        var result = await warehouseService.CreateLocation(location);
        return result.Result ? location : null;
    }
}
