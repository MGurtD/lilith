using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase;

public class SupplierTypeRepository(ApplicationDbContext context) : Repository<SupplierType, Guid>(context), ISupplierTypeRepository
{
    // Suppliers reference their type with a cascading foreign key, and their
    // invoices and orders cascade from them, so deleting a type in use would
    // silently delete all of them.
    public async Task<bool> IsInUse(Guid supplierTypeId)
    {
        return await context.Set<Supplier>().AnyAsync(s => s.SupplierTypeId == supplierTypeId);
    }
}
