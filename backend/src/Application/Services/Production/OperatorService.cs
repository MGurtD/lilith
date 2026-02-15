using Application.Contracts;
using Application.Services;
using Domain.Entities.Production;

namespace Application.Services.Production;

public class OperatorService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IOperatorService
{
    public async Task<Operator?> GetById(Guid id)
    {
        return await unitOfWork.Operators.Get(id);
    }

    public async Task<IEnumerable<Operator>> GetAll()
    {
        var entities = await unitOfWork.Operators.GetAll();
        return entities.OrderBy(o => o.Name);
    }

    public async Task<GenericResponse> Create(Operator operatorEntity)
    {
        var exists = unitOfWork.Operators.Find(o => o.Code == operatorEntity.Code).Any();
        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("OperatorAlreadyExists", operatorEntity.Name));
        }

        await unitOfWork.Operators.Add(operatorEntity);
        return new GenericResponse(true, operatorEntity);
    }

    public async Task<GenericResponse> Update(Operator operatorEntity)
    {
        var exists = await unitOfWork.Operators.Exists(operatorEntity.Id);
        if (!exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", operatorEntity.Id));
        }

        await unitOfWork.Operators.Update(operatorEntity);
        return new GenericResponse(true, operatorEntity);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.Operators.Find(o => o.Id == id).FirstOrDefault();
        if (entity == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.Operators.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
