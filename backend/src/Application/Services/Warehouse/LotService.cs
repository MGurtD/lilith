using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Services.Warehouse;

public class LotService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ILotService
{
    public async Task<GenericResponse> Create(Lot lot)
    {
        var exists = await unitOfWork.Lots.Exists(lot.Id);
        if (exists)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityAlreadyExists"));

        await unitOfWork.Lots.Add(lot);
        return new GenericResponse(true, lot);
    }

    public async Task<IEnumerable<Lot>> GetAll()
    {
        var lots = await unitOfWork.Lots.GetAll();
        return lots.OrderBy(l => l.Code);
    }

    public async Task<Lot?> GetById(Guid id)
    {
        return await unitOfWork.Lots.Get(id);
    }

    public async Task<GenericResponse> Update(Lot lot)
    {
        var exists = await unitOfWork.Lots.Exists(lot.Id);
        if (!exists)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", lot.Id));

        await unitOfWork.Lots.Update(lot);
        return new GenericResponse(true, lot);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var lot = await unitOfWork.Lots.Get(id);
        if (lot is null)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", id));

        await unitOfWork.Lots.Remove(lot);
        return new GenericResponse(true, lot);
    }

    public Task<IEnumerable<Lot>> GetOpenLotsByReference(Guid referenceId)
    {
        IEnumerable<Lot> lots = unitOfWork.Lots
            .Find(l => l.ReferenceId == referenceId && l.ClosedDate == null)
            .OrderBy(l => l.Code);
        return Task.FromResult(lots);
    }

    public async Task<GenericResponse> ResolveOrCreateLot(Guid referenceId, string? code, string? supplierLotCode, DateTime? expirationDate)
    {
        var normalizedCode = code ?? string.Empty;

        // Nomes es reutilitza un lot obert; un lot tancat amb el mateix codi mai es reobre
        var existingLot = unitOfWork.Lots
            .Find(l => l.ReferenceId == referenceId && l.Code == normalizedCode && l.ClosedDate == null)
            .FirstOrDefault();
        if (existingLot != null)
            return new GenericResponse(true, existingLot);

        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            ReferenceId = referenceId,
            Code = normalizedCode,
            SupplierLotCode = supplierLotCode,
            ExpirationDate = expirationDate
        };
        await unitOfWork.Lots.Add(lot);

        return new GenericResponse(true, lot);
    }
}
