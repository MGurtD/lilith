using Domain.Entities.Sales;

namespace Application.Contracts;

public interface ICustomerTypeRepository : IRepository<CustomerType, Guid>
{
    /// <summary>
    /// True when a customer uses the customer type.
    /// </summary>
    Task<bool> IsInUse(Guid customerTypeId);
}
