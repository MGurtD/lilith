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
        IEnumerable<Supplier> GetLogisticSuppliers();
        Task<SupplierReference?> GetSupplierReferenceBySupplierAndId(Guid referenceId, Guid supplierId);

        Task<List<SupplierReference>> GetSuppliersReferencesFromReference(Guid referenceId);
        Task AddSupplierReference(SupplierReference reference);
        Task UpdateSupplierReference(SupplierReference reference);
        Task RemoveSupplierReference(SupplierReference reference);

        Task<List<string>> GetAccountNumbersUsedInPurchaseInvoices();

        /// <summary>
        /// True when a purchase order, receipt, purchase invoice, purchase rate,
        /// transport rate, or a budget or sales order (external service or
        /// transport) uses the supplier. Contacts and reference links do not count.
        /// </summary>
        Task<bool> IsInUse(Guid supplierId);
    }
}
