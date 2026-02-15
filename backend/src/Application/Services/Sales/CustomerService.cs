using Application.Contracts;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Sales;

namespace Application.Services.Sales;

public class CustomerService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : ICustomerService
{
    // Customer CRUD operations
    public async Task<IEnumerable<Customer>> GetAllCustomers()
    {
        var customers = await unitOfWork.Customers.GetAll();
        return customers.OrderBy(c => c.ComercialName);
    }

    public async Task<Customer?> GetCustomerById(Guid id)
    {
        return await unitOfWork.Customers.Get(id);
    }

    public async Task<GenericResponse> CreateCustomer(Customer customer)
    {
        var exists = unitOfWork.Customers
            .Find(c => c.ComercialName == customer.ComercialName)
            .Any();

        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("CustomerAlreadyExists"));
        }

        await unitOfWork.Customers.Add(customer);
        return new GenericResponse(true, customer);
    }

    public async Task<GenericResponse> UpdateCustomer(Customer customer)
    {
        var exists = await unitOfWork.Customers.Get(customer.Id);
        if (exists == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", customer.Id));
        }

        await unitOfWork.Customers.Update(customer);
        return new GenericResponse(true, customer);
    }

    public async Task<GenericResponse> RemoveCustomer(Guid id)
    {
        var customer = unitOfWork.Customers.Find(c => c.Id == id).FirstOrDefault();
        if (customer == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.Customers.Remove(customer);
        return new GenericResponse(true, customer);
    }

    // Contact operations
    public async Task<GenericResponse> CreateContact(CustomerContact contact)
    {
        var customer = await unitOfWork.Customers.Get(contact.CustomerId);
        if (customer == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("CustomerNotFound"));
        }

        await unitOfWork.Customers.AddContact(contact);
        return new GenericResponse(true, contact);
    }

    public async Task<GenericResponse> UpdateContact(Guid id, CustomerContact contact)
    {
        var existingContact = unitOfWork.Customers.GetContactById(id);
        if (existingContact == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        // Map properties
        existingContact.FirstName = contact.FirstName;
        existingContact.LastName = contact.LastName;
        existingContact.Charge = contact.Charge;
        existingContact.Email = contact.Email;
        existingContact.PhoneNumber = contact.PhoneNumber;
        existingContact.Extension = contact.Extension;
        existingContact.Main = contact.Main;

        existingContact.Disabled = contact.Disabled;

        await unitOfWork.Customers.UpdateContact(existingContact);
        return new GenericResponse(true, existingContact);
    }

    public async Task<GenericResponse> RemoveContact(Guid id)
    {
        var contact = unitOfWork.Customers.GetContactById(id);
        if (contact == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.Customers.RemoveContact(contact);
        return new GenericResponse(true, contact);
    }

    // Address operations
    public async Task<GenericResponse> CreateAddress(CustomerAddress address)
    {
        var customer = await unitOfWork.Customers.Get(address.CustomerId);
        if (customer == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("CustomerNotFound"));
        }

        await unitOfWork.Customers.AddAddress(address);
        return new GenericResponse(true, address);
    }

    public async Task<GenericResponse> UpdateAddress(Guid id, CustomerAddress address)
    {
        var existingAddress = unitOfWork.Customers.GetAddressById(id);
        if (existingAddress == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        // Map properties
        existingAddress.Name = address.Name;
        existingAddress.Address = address.Address;
        existingAddress.Country = address.Country;
        existingAddress.Region = address.Region;
        existingAddress.PostalCode = address.PostalCode;
        existingAddress.City = address.City;
        existingAddress.Disabled = address.Disabled;
        existingAddress.Main = address.Main;
        existingAddress.Observations = address.Observations;

        await unitOfWork.Customers.UpdateAddress(existingAddress);
        return new GenericResponse(true, existingAddress);
    }

    public async Task<GenericResponse> RemoveAddress(Guid id)
    {
        var address = unitOfWork.Customers.GetAddressById(id);
        if (address == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.Customers.RemoveAddress(address);
        return new GenericResponse(true, address);
    }
}
