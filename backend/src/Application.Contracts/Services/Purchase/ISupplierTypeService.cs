using Domain.Entities.Purchase;

namespace Application.Contracts;

public interface ISupplierTypeService
{
    Task<IEnumerable<SupplierType>> GetAllSupplierTypes();
    Task<SupplierType?> GetSupplierTypeById(Guid id);
    Task<GenericResponse> CreateSupplierType(SupplierType supplierType);
    Task<GenericResponse> UpdateSupplierType(SupplierType supplierType);
    Task<GenericResponse> RemoveSupplierType(Guid id);
}
