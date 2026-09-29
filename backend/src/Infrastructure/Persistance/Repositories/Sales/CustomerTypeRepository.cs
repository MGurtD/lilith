using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Sales;

public class CustomerTypeRepository(ApplicationDbContext context) : Repository<CustomerType, Guid>(context), ICustomerTypeRepository
{
    // Customers reference their type with a cascading foreign key, and their
    // delivery notes cascade from them, so deleting a type in use would silently
    // delete all of them.
    public async Task<bool> IsInUse(Guid customerTypeId)
    {
        return await context.Set<Customer>().AnyAsync(c => c.CustomerTypeId == customerTypeId);
    }
}
