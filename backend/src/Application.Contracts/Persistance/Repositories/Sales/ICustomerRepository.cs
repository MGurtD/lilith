using Domain.Entities.Sales;

namespace Application.Contracts
{
    public interface ICustomerRepository : IRepository<Customer, Guid>
    {
        CustomerContact? GetContactById(Guid id);
        Task AddContact(CustomerContact contact);
        Task UpdateContact(CustomerContact contact);
        Task RemoveContact(CustomerContact contact);

        CustomerAddress? GetAddressById(Guid id);
        Task AddAddress(CustomerAddress address);
        Task UpdateAddress(CustomerAddress address);
        Task RemoveAddress(CustomerAddress address);
    }
}
