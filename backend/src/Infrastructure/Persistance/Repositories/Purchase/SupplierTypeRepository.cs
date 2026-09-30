using Application.Contracts;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase;

public class SupplierTypeRepository(ApplicationDbContext context) : Repository<SupplierType, Guid>(context), ISupplierTypeRepository
{
}
