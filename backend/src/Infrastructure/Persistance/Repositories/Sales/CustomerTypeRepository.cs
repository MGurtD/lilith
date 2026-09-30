using Application.Contracts;
using Domain.Entities.Sales;

namespace Infrastructure.Persistance.Repositories.Sales;

public class CustomerTypeRepository(ApplicationDbContext context) : Repository<CustomerType, Guid>(context), ICustomerTypeRepository
{
}
