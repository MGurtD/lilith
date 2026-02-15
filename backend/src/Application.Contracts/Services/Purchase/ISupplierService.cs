using Domain.Entities.Purchase;

namespace Application.Contracts;

public interface ISupplierService
{
    // Supplier CRUD
    Task<Supplier?> GetSupplierById(Guid id);
    Task<IEnumerable<Supplier>> GetAllSuppliers();
    Task<GenericResponse> CreateSupplier(Supplier supplier);
    Task<GenericResponse> UpdateSupplier(Supplier supplier);
    Task<GenericResponse> RemoveSupplier(Guid id);

    // Contact operations
    Task<GenericResponse> CreateContact(SupplierContact contact);
    Task<GenericResponse> UpdateContact(Guid id, SupplierContact contact);
    Task<GenericResponse> RemoveContact(Guid id);

    // SupplierReference operations
    Task<SupplierReference?> GetSupplierReferenceBySupplierIdAndReferenceId(Guid supplierId, Guid referenceId);
    Task<SupplierReference?> GetSupplierReferenceById(Guid supplierReferenceId);
    IEnumerable<SupplierReference> GetSupplierReferences(Guid supplierId);
    IEnumerable<Supplier> GetSuppliersByReference(Guid referenceId);
    Task<GenericResponse> CreateSupplierReference(SupplierReference supplierReference);
    Task<GenericResponse> UpdateSupplierReference(Guid id, SupplierReference supplierReference);
    Task<GenericResponse> RemoveSupplierReference(Guid id);
}
