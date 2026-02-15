using Domain.Entities.Purchase;

namespace Application.Contracts
{
    public interface ISupplierRepository : IRepository<Supplier, Guid>
    {
        SupplierContact? GetContactById(Guid id);
        Task AddContact(SupplierContact contact);
        Task UpdateContact(SupplierContact contact);
        Task RemoveContact(SupplierContact contact);

        Task<SupplierReference?> GetSupplierReferenceById(Guid id);
        Task<SupplierReference?> GetSupplierReferenceBySupplierIdAndReferenceId(Guid supplierId, Guid referenceId);
        IEnumerable<SupplierReference> GetSupplierReferences(Guid supplierReferenceId);
        IEnumerable<Supplier> GetReferenceSuppliers(Guid referenceId);
        Task<SupplierReference?> GetSupplierReferenceBySupplierAndId(Guid referenceId, Guid supplierId);

        Task<List<SupplierReference>> GetSuppliersReferencesFromReference(Guid referenceId);
        Task AddSupplierReference(SupplierReference reference);
        Task UpdateSupplierReference(SupplierReference reference);
        Task RemoveSupplierReference(SupplierReference reference);
    }
}
