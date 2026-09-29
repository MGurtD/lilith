using Application.Contracts;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Sales
{
    public class CustomerRepository : Repository<Customer, Guid>, ICustomerRepository
    {
        private readonly IRepository<CustomerContact, Guid> _customerContactRepository;
        private readonly IRepository<CustomerAddress, Guid> _customerAddressRepository;

        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
            _customerContactRepository = new Repository<CustomerContact, Guid>(context);
            _customerAddressRepository = new Repository<CustomerAddress, Guid>(context);
        }

        public override async Task<Customer?> Get(Guid id)
        {
            return await dbSet.Include(s => s.Contacts).Include(s => s.Address).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public CustomerContact? GetContactById(Guid id)
        {
            var contact = _customerContactRepository.Find(c => c.Id == id).FirstOrDefault();
            return contact;
        }

        public async Task AddContact(CustomerContact contact)
        {
            await _customerContactRepository.Add(contact);
        }

        public async Task RemoveContact(CustomerContact contact)
        {
            await _customerContactRepository.Remove(contact);
        }

        public async Task UpdateContact(CustomerContact contact)
        {
            await _customerContactRepository.Update(contact);
        }

        public CustomerAddress? GetAddressById(Guid id)
        {
            var address = _customerAddressRepository.Find(c => c.Id == id).FirstOrDefault();
            return address;
        }

        public async Task AddAddress(CustomerAddress address)
        {
            await _customerAddressRepository.Add(address);
        }

        public async Task RemoveAddress(CustomerAddress address)
        {
            await _customerAddressRepository.Remove(address);
        }

        public async Task UpdateAddress(CustomerAddress address)
        {
            await _customerAddressRepository.Update(address);
        }

        // Delivery notes reference the customer with a cascading foreign key, so
        // deleting a customer in use would silently delete them; the other documents
        // and references would block the delete.
        public async Task<bool> IsInUse(Guid customerId)
        {
            return await context.Set<Budget>().AnyAsync(e => e.CustomerId == customerId)
                || await context.Set<SalesOrderHeader>().AnyAsync(e => e.CustomerId == customerId)
                || await context.Set<DeliveryNote>().AnyAsync(e => e.CustomerId == customerId)
                || await context.Set<SalesInvoice>().AnyAsync(e => e.CustomerId == customerId)
                || await context.Set<Reference>().AnyAsync(e => e.CustomerId == customerId);
        }
    }
}
