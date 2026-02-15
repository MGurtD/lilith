using Application.Contracts;
using Domain.Entities.Sales;

namespace Application.Contracts;

public interface ICustomerTypeService
{
    Task<IEnumerable<CustomerType>> GetAllCustomerTypes();
    Task<CustomerType?> GetCustomerTypeById(Guid id);
    Task<GenericResponse> CreateCustomerType(CustomerType customerType);
    Task<GenericResponse> UpdateCustomerType(CustomerType customerType);
    Task<GenericResponse> RemoveCustomerType(Guid id);
}
