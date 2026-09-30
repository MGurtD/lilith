using Application.Contracts;
using Application.Services;
using Domain.Entities.Purchase;

namespace Application.Services.Purchase;

public class SupplierTypeService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : ISupplierTypeService
{
    public async Task<IEnumerable<SupplierType>> GetAllSupplierTypes()
    {
        var entities = await unitOfWork.SupplierTypes.GetAll();
        return entities.OrderBy(e => e.Name);
    }

    public async Task<SupplierType?> GetSupplierTypeById(Guid id)
    {
        return await unitOfWork.SupplierTypes.Get(id);
    }

    public async Task<GenericResponse> CreateSupplierType(SupplierType supplierType)
    {
        var exists = unitOfWork.SupplierTypes.Find(r => r.Name == supplierType.Name).Any();
        if (exists)
        {
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("EntityAlreadyExists"));
        }

        await unitOfWork.SupplierTypes.Add(supplierType);
        return new GenericResponse(true, supplierType);
    }

    public async Task<GenericResponse> UpdateSupplierType(SupplierType supplierType)
    {
        var exists = await unitOfWork.SupplierTypes.Exists(supplierType.Id);
        if (!exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", supplierType.Id));
        }

        await unitOfWork.SupplierTypes.Update(supplierType);
        return new GenericResponse(true, supplierType);
    }

    public async Task<GenericResponse> RemoveSupplierType(Guid id)
    {
        var entity = unitOfWork.SupplierTypes.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.SupplierTypes.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
