using Domain.Entities.Purchase;

namespace Application.Contracts;

public interface ISupplierTypeRepository : IRepository<SupplierType, Guid>
{
    /// <summary>
    /// True when a supplier uses the supplier type.
    /// </summary>
    Task<bool> IsInUse(Guid supplierTypeId);
}
