using Application.Contracts;
using Application.Services;
using Domain.Entities.Production;

namespace Application.Services.Production;

public class WorkcenterService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IWorkcenterService
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
        return new GenericResponse(true, workcenter);
    }

    public async Task<GenericResponse> Update(Workcenter workcenter)
    {
        var exists = await unitOfWork.Workcenters.Exists(workcenter.Id);
        if (!exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", workcenter.Id));
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

        await unitOfWork.Workcenters.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
