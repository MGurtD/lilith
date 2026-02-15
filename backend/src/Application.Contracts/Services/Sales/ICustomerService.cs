using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Sales;

namespace Application.Contracts;

public interface ICustomerService
{
    // Customer CRUD operations
    Task<IEnumerable<Customer>> GetAllCustomers();
    Task<Customer?> GetCustomerById(Guid id);
    Task<GenericResponse> CreateCustomer(Customer customer);
    Task<GenericResponse> UpdateCustomer(Customer customer);
    Task<GenericResponse> RemoveCustomer(Guid id);

    // Contact operations
    Task<GenericResponse> CreateContact(CustomerContact contact);
    Task<GenericResponse> UpdateContact(Guid id, CustomerContact contact);
    Task<GenericResponse> RemoveContact(Guid id);

    // Address operations
    Task<GenericResponse> CreateAddress(CustomerAddress address);
    Task<GenericResponse> UpdateAddress(Guid id, CustomerAddress address);
    Task<GenericResponse> RemoveAddress(Guid id);
}
