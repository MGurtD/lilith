using Application.Contracts;
using Domain.Entities.Purchase;

namespace Application.Services.Purchase;

public class PurchaseRateService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IPurchaseRateService
{
    public async Task<IEnumerable<PurchaseRate>> GetPurchaseRatesBySupplierId(Guid supplierId)
    {
        var rates = await unitOfWork.PurchaseRates.FindAsync(r => r.SupplierId == supplierId);
        return rates.OrderBy(r => r.ValidFrom);
    }

    public async Task<IEnumerable<PurchaseRate>> GetPurchaseRatesByReferenceId(Guid referenceId)
    {
        return await unitOfWork.PurchaseRates.GetByReferenceId(referenceId);
    }

    public async Task<PurchaseRate?> GetPurchaseRateById(Guid id)
    {
        return await unitOfWork.PurchaseRates.Get(id);
    }

    public async Task<GenericResponse> CreatePurchaseRate(PurchaseRate purchaseRate)
    {
        // Comprovar encavalcament de dates per al mateix proveïdor
        var overlapping = await unitOfWork.PurchaseRates.FindAsync(r =>
            r.SupplierId == purchaseRate.SupplierId &&
            r.Id != purchaseRate.Id &&
            r.ValidFrom <= purchaseRate.ValidTo &&
            r.ValidTo >= purchaseRate.ValidFrom);

        if (overlapping.Any())
            return new GenericResponse(false, localizationService.GetLocalizedString("PurchaseRateDateOverlap"));

        await unitOfWork.PurchaseRates.Add(purchaseRate);
        return new GenericResponse(true, purchaseRate);
    }

    public async Task<GenericResponse> UpdatePurchaseRate(Guid id, PurchaseRate purchaseRate)
    {
        if (id != purchaseRate.Id)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound"));

        var exists = await unitOfWork.PurchaseRates.Exists(id);
        if (!exists)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound"));

        // Comprovar encavalcament de dates per al mateix proveïdor (excloent l'actual)
        var overlapping = await unitOfWork.PurchaseRates.FindAsync(r =>
            r.SupplierId == purchaseRate.SupplierId &&
            r.Id != purchaseRate.Id &&
            r.ValidFrom <= purchaseRate.ValidTo &&
            r.ValidTo >= purchaseRate.ValidFrom);

        if (overlapping.Any())
            return new GenericResponse(false, localizationService.GetLocalizedString("PurchaseRateDateOverlap"));

        await unitOfWork.PurchaseRates.Update(purchaseRate);
        return new GenericResponse(true, purchaseRate);
    }

    public async Task<GenericResponse> RemovePurchaseRate(Guid id)
    {
        var entity = await unitOfWork.PurchaseRates.Get(id);
        if (entity is null)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound"));

        await unitOfWork.PurchaseRates.Remove(entity);
        return new GenericResponse(true, entity);
    }

    public async Task<GenericResponse> DuplicatePurchaseRate(Guid sourceId, string newName, DateOnly newValidFrom, DateOnly newValidTo)
    {
        var source = await unitOfWork.PurchaseRates.Get(sourceId);
        if (source is null)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound"));

        // Comprovar encavalcament de dates per al mateix proveïdor
        var overlapping = await unitOfWork.PurchaseRates.FindAsync(r =>
            r.SupplierId == source.SupplierId &&
            r.ValidFrom <= newValidTo &&
            r.ValidTo >= newValidFrom);

        if (overlapping.Any())
            return new GenericResponse(false, localizationService.GetLocalizedString("PurchaseRateDateOverlap"));

        var newRate = new PurchaseRate
        {
            Id = Guid.NewGuid(),
            Name = newName,
            SupplierId = source.SupplierId,
            ValidFrom = newValidFrom,
            ValidTo = newValidTo,
        };

        await unitOfWork.PurchaseRates.Add(newRate);

        // Copiar els detalls de l'original
        if (source.Details != null)
        {
            foreach (var detail in source.Details)
            {
                var newDetail = new PurchaseRateDetail
                {
                    Id = Guid.NewGuid(),
                    PurchaseRateId = newRate.Id,
                    ReferenceId = detail.ReferenceId,
                    From = detail.From,
                    To = detail.To,
                    CalculationType = detail.CalculationType,
                    Price = detail.Price,
                };
                await unitOfWork.PurchaseRateDetails.Add(newDetail);
            }
        }

        return new GenericResponse(true, newRate);
    }

    public async Task<IEnumerable<PurchaseRateDetail>> GetPurchaseRateDetails(Guid purchaseRateId)
    {
        return await unitOfWork.PurchaseRateDetails.FindAsync(d => d.PurchaseRateId == purchaseRateId);
    }

    public async Task<GenericResponse> CreatePurchaseRateDetail(PurchaseRateDetail detail)
    {
        await unitOfWork.PurchaseRateDetails.Add(detail);
        return new GenericResponse(true, detail);
    }

    public async Task<GenericResponse> UpdatePurchaseRateDetail(PurchaseRateDetail detail)
    {
        await unitOfWork.PurchaseRateDetails.Update(detail);
        return new GenericResponse(true, detail);
    }

    public async Task<GenericResponse> RemovePurchaseRateDetail(Guid id)
    {
        var entity = await unitOfWork.PurchaseRateDetails.Get(id);
        if (entity is null)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound"));

        await unitOfWork.PurchaseRateDetails.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
