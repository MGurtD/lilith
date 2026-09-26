using Application.Contracts;
using Domain.Entities.Shared;

namespace Application.Services.Shared;

public class ParameterService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IParameterService
{
    public async Task<IEnumerable<Parameter>> GetAll()
    {
        var entities = await unitOfWork.Parameters.GetAll();
        return entities.OrderBy(e => e.Key);
    }

    public async Task<Parameter?> GetById(Guid id)
    {
        return await unitOfWork.Parameters.Get(id);
    }

    public async Task<GenericResponse> Create(Parameter parameter)
    {
        var exists = unitOfWork.Parameters.Find(p => p.Key == parameter.Key).Any();
        if (exists)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityAlreadyExists"));

        await unitOfWork.Parameters.Add(parameter);
        return new GenericResponse(true, parameter);
    }

    public async Task<GenericResponse> Update(Parameter parameter)
    {
        var exists = await unitOfWork.Parameters.Exists(parameter.Id);
        if (!exists)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", parameter.Id));

        await unitOfWork.Parameters.Update(parameter);
        return new GenericResponse(true, parameter);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.Parameters.Find(p => p.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", id));

        await unitOfWork.Parameters.Remove(entity);
        return new GenericResponse(true, entity);
    }

    public async Task<bool> GetBool(string key, bool defaultValue)
    {
        var parameter = unitOfWork.Parameters.Find(p => p.Key == key).FirstOrDefault();
        if (parameter is null)
            return defaultValue;

        return bool.TryParse(parameter.Value, out var value) ? value : defaultValue;
    }
}
