using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Production;

public class WorkcenterLocationService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IWorkcenterLocationService
{
    public async Task<WorkcenterLocation?> GetById(Guid id)
    {
        return await unitOfWork.WorkcenterLocations.Get(id);
    }

    public async Task<IEnumerable<WorkcenterLocation>> GetAll()
    {
        var entities = await unitOfWork.WorkcenterLocations.GetAll();
        return entities.OrderBy(w => w.WorkcenterId);
    }

    public async Task<IEnumerable<WorkcenterLocation>> GetByWorkcenterId(Guid workcenterId)
    {
        return await unitOfWork.WorkcenterLocations.FindAsyncWithQueryParams(
            wl => wl.WorkcenterId == workcenterId,
            q => q.Include(wl => wl.Location)
        );
    }

    public async Task<GenericResponse> Create(WorkcenterLocation workcenterLocation)
    {
        var exists = unitOfWork.WorkcenterLocations.Find(wl => wl.Id == workcenterLocation.Id).Any();
        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityAlreadyExists", workcenterLocation.Id));
        }

        await unitOfWork.WorkcenterLocations.Add(workcenterLocation);
        return new GenericResponse(true, workcenterLocation);
    }

    public async Task<GenericResponse> Update(WorkcenterLocation workcenterLocation)
    {
        var exists = await unitOfWork.WorkcenterLocations.Exists(workcenterLocation.Id);
        if (!exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", workcenterLocation.Id));
        }

        await unitOfWork.WorkcenterLocations.Update(workcenterLocation);
        return new GenericResponse(true, workcenterLocation);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.WorkcenterLocations.Find(wl => wl.Id == id).FirstOrDefault();
        if (entity == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.WorkcenterLocations.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
