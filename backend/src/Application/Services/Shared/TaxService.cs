using Application.Contracts;
using Domain.Entities;

namespace Application.Services.Shared;

public class TaxService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ITaxService
{
    public async Task<GenericResponse> CreateTax(Tax tax)
    {
        var exists = unitOfWork.Taxes.Find(e => e.Id == tax.Id).Any();
        if (exists)
        {
            var message = localizationService.GetLocalizedString("EntityAlreadyExists");
            return new GenericResponse(false, message);
        }

        await unitOfWork.Taxes.Add(tax);
        return new GenericResponse(true, tax);
    }

    public async Task<IEnumerable<Tax>> GetAllTaxes()
    {
        var entities = await unitOfWork.Taxes.GetAll();
        return entities.OrderBy(e => e.Name);
    }

    public async Task<Tax?> GetTaxById(Guid id)
    {
        return await unitOfWork.Taxes.Get(id);
    }

    public async Task<GenericResponse> UpdateTax(Tax tax)
    {
        var exists = await unitOfWork.Taxes.Exists(tax.Id);
        if (!exists)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", tax.Id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Taxes.Update(tax);
        return new GenericResponse(true, tax);
    }

    public async Task<GenericResponse> RemoveTax(Guid id)
    {
        var entity = unitOfWork.Taxes.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Taxes.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
