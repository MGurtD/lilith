using Application.Contracts;
using Domain.Entities.Shared;

namespace Application.Services.Shared;

public class ReferenceTypeService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IReferenceTypeService
{
    public async Task<GenericResponse> CreateReferenceType(ReferenceType referenceType)
    {
        var exists = unitOfWork.ReferenceTypes.Find(e => e.Id == referenceType.Id).Any();
        if (exists)
        {
            var message = localizationService.GetLocalizedString("EntityAlreadyExists");
            return new GenericResponse(false, message);
        }

        await unitOfWork.ReferenceTypes.Add(referenceType);
        return new GenericResponse(true, referenceType);
    }

    public async Task<IEnumerable<ReferenceType>> GetAllReferenceTypes()
    {
        var entities = await unitOfWork.ReferenceTypes.GetAll();
        return entities.OrderBy(e => e.Name);
    }

    public async Task<ReferenceType?> GetReferenceTypeById(Guid id)
    {
        return await unitOfWork.ReferenceTypes.Get(id);
    }

    public async Task<GenericResponse> UpdateReferenceType(ReferenceType referenceType)
    {
        var exists = await unitOfWork.ReferenceTypes.Exists(referenceType.Id);
        if (!exists)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", referenceType.Id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.ReferenceTypes.Update(referenceType);
        return new GenericResponse(true, referenceType);
    }

    public async Task<GenericResponse> RemoveReferenceType(Guid id)
    {
        var entity = unitOfWork.ReferenceTypes.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.ReferenceTypes.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
