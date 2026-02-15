using Application.Contracts;
using Application.Services;
using Domain.Entities.Shared;

namespace Application.Services.Shared;

public class InvoiceSerieService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IInvoiceSerieService
{
    public async Task<IEnumerable<InvoiceSerie>> GetAllInvoiceSeries()
    {
        return await unitOfWork.InvoiceSeries.GetAll();
    }

    public async Task<InvoiceSerie?> GetInvoiceSerieById(Guid id)
    {
        return await unitOfWork.InvoiceSeries.Get(id);
    }

    public async Task<GenericResponse> CreateInvoiceSerie(InvoiceSerie invoiceSerie)
    {
        var exists = unitOfWork.InvoiceSeries.Find(r => r.Name == invoiceSerie.Name).Any();
        if (exists)
        {
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("EntityAlreadyExists"));
        }

        await unitOfWork.InvoiceSeries.Add(invoiceSerie);
        return new GenericResponse(true, invoiceSerie);
    }

    public async Task<GenericResponse> UpdateInvoiceSerie(InvoiceSerie invoiceSerie)
    {
        var exists = await unitOfWork.InvoiceSeries.Exists(invoiceSerie.Id);
        if (!exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", invoiceSerie.Id));
        }

        await unitOfWork.InvoiceSeries.Update(invoiceSerie);
        return new GenericResponse(true, invoiceSerie);
    }

    public async Task<GenericResponse> RemoveInvoiceSerie(Guid id)
    {
        var entity = unitOfWork.InvoiceSeries.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.InvoiceSeries.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
