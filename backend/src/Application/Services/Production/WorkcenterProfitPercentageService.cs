using Application.Contracts;
using Application.Services;
using Domain.Entities.Production;

namespace Application.Services.Production;

public class WorkcenterProfitPercentageService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IWorkcenterProfitPercentageService
{
    public async Task<WorkcenterProfitPercentage?> GetById(Guid id)
    {
        return await unitOfWork.WorkcenterProfitPercentages.Get(id);
    }

    public async Task<IEnumerable<WorkcenterProfitPercentage>> GetAll()
    {
        var entities = await unitOfWork.WorkcenterProfitPercentages.GetAll();
        return entities.OrderBy(w => w.WorkcenterId);
    }

    public async Task<IEnumerable<WorkcenterProfitPercentage>> GetByWorkcenterId(Guid workcenterId)
    {
        return await unitOfWork.WorkcenterProfitPercentages.GetByWorkcenterId(workcenterId);
    }

    public async Task<GenericResponse> Create(WorkcenterProfitPercentage workcenterProfitPercentage)
    {
        var exists = unitOfWork.WorkcenterProfitPercentages.Find(w => w.Id == workcenterProfitPercentage.Id).Any();
        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("WorkcenterProfitPercentageAlreadyExists", workcenterProfitPercentage.Id));
        }

        await unitOfWork.WorkcenterProfitPercentages.Add(workcenterProfitPercentage);
        return new GenericResponse(true, workcenterProfitPercentage);
    }

    public async Task<GenericResponse> Update(WorkcenterProfitPercentage workcenterProfitPercentage)
    {
        var exists = await unitOfWork.WorkcenterProfitPercentages.Exists(workcenterProfitPercentage.Id);
        if (!exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", workcenterProfitPercentage.Id));
        }

        await unitOfWork.WorkcenterProfitPercentages.Update(workcenterProfitPercentage);
        return new GenericResponse(true, workcenterProfitPercentage);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.WorkcenterProfitPercentages.Find(w => w.Id == id).FirstOrDefault();
        if (entity == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.WorkcenterProfitPercentages.Remove(entity);
        return new GenericResponse(true, entity);
    }
}