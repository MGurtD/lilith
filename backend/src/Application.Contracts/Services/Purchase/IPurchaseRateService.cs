using Domain.Entities.Purchase;

namespace Application.Contracts;

public interface IPurchaseRateService
{
    Task<IEnumerable<PurchaseRate>> GetPurchaseRatesBySupplierId(Guid supplierId);
    Task<IEnumerable<PurchaseRate>> GetPurchaseRatesByReferenceId(Guid referenceId);
    Task<PurchaseRate?> GetPurchaseRateById(Guid id);
    Task<GenericResponse> CreatePurchaseRate(PurchaseRate purchaseRate);
    Task<GenericResponse> UpdatePurchaseRate(Guid id, PurchaseRate purchaseRate);
    Task<GenericResponse> RemovePurchaseRate(Guid id);
    Task<GenericResponse> DuplicatePurchaseRate(Guid sourceId, string newName, DateOnly newValidFrom, DateOnly newValidTo);

    Task<IEnumerable<PurchaseRateDetail>> GetPurchaseRateDetails(Guid purchaseRateId);
    Task<GenericResponse> CreatePurchaseRateDetail(PurchaseRateDetail detail);
    Task<GenericResponse> UpdatePurchaseRateDetail(PurchaseRateDetail detail);
    Task<GenericResponse> RemovePurchaseRateDetail(Guid id);
}
