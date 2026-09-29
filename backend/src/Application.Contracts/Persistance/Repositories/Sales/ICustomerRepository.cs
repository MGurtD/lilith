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

        /// <summary>
        /// True when a budget, sales order, delivery note, sales invoice or
        /// reference uses the customer. Contacts and addresses do not count.
        /// </summary>
        Task<bool> IsInUse(Guid customerId);
    }
}
