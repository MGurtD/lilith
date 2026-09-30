using Domain.Entities.Sales;

namespace Application.Contracts;

public interface ICustomerTypeRepository : IRepository<CustomerType, Guid>
{
}
