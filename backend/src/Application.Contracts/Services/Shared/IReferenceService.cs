using Application.Contracts;
using Domain.Entities.Purchase;
using Domain.Entities.Shared;

namespace Application.Contracts;
public interface IReferenceService
{
    // Read operations - return entities directly
    Task<IEnumerable<Reference>> GetAllReferences();
    Task<IEnumerable<Reference>> GetAllSales();
    Task<IEnumerable<Reference>> GetAllSalesByCustomerId(Guid customerId);
    Task<IEnumerable<Reference>> GetAllPurchase(string? categoryName = null);
    Task<IEnumerable<Reference>> GetAllProduction();
    Task<Reference?> GetReferenceById(Guid id);
    Task<IEnumerable<SupplierReference>> GetReferenceSuppliers(Guid referenceId);
    Task<IEnumerable<ReferenceFormat>> GetReferenceFormats();
    Task<ReferenceFormat?> GetReferenceFormatById(Guid id);

    // Write operations - return GenericResponse
    Task<GenericResponse> CreateReference(Reference reference);
    Task<GenericResponse> UpdateReference(Reference reference);
    Task<GenericResponse> RemoveReference(Guid id);

    // Existing methods
    GenericResponse CanDelete(Guid referenceId);
    Task<List<Reference>> GetReferenceByCategory(string categoryName);
    Task<GenericResponse> UpdatePriceFromReceipt(Receipt receipt);
    Task<decimal> GetPrice(Guid referenceId, Guid? supplierId);
}
