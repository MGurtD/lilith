using Application.Contracts;
using Application.Contracts.Services.Geolocalization;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Sales;
using Microsoft.Extensions.Logging;

namespace Application.Services.Sales;

public class CustomerService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService,
    IGeolocalizationService geolocalizationService,
    ILogger<CustomerService> logger) : ICustomerService
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

        await UpdateCoordinatesAndDistanceAsync(address);

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
        existingAddress.Latitude = address.Latitude;
        existingAddress.Longitude = address.Longitude;

        await UpdateCoordinatesAndDistanceAsync(existingAddress);

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

    private async Task UpdateCoordinatesAndDistanceAsync(CustomerAddress address)
    {
        logger.LogInformation("UpdateCoordinatesAndDistanceAsync -> CustomerAddress: {AddressId} | Lat: {Lat}, Lon: {Lon}",
            address.Id, address.Latitude, address.Longitude);

        // 1. Si el frontend ya proporcionó coordenadas (vía AutocompleteLocation), confiar en ellas.
        //    Si no, intentar geocodificar como fallback.
        if (address.Latitude == 0 && address.Longitude == 0)
        {
            if (!string.IsNullOrWhiteSpace(address.Address) &&
                !string.IsNullOrWhiteSpace(address.City) &&
                !string.IsNullOrWhiteSpace(address.Country))
            {
                logger.LogInformation("Geocoding address {AddressId}: {Address}, {City}, {Country}",
                    address.Id, address.Address, address.City, address.Country);

                var coords = await geolocalizationService.GetCoordinatesAsync(
                    address.Address, address.City, address.PostalCode, address.Country);
                if (coords != null)
                {
                    address.Latitude = coords.Latitude;
                    address.Longitude = coords.Longitude;
                    logger.LogInformation("Geocoding address {AddressId} resolved -> Lat: {Lat}, Lon: {Lon}",
                        address.Id, coords.Latitude, coords.Longitude);
                }
                else
                {
                    logger.LogWarning("Geocoding address {AddressId} returned no results.", address.Id);
                }
            }
            else
            {
                logger.LogWarning("CustomerAddress {AddressId} has no coordinates and insufficient address data to geocode.", address.Id);
            }
        }

        // 2. Si tenemos coordenadas válidas, calcular distancia desde el site por defecto
        if (address.Latitude != 0 && address.Longitude != 0)
        {
            var defaultSite = (await unitOfWork.Sites.GetAll()).FirstOrDefault();
            if (defaultSite != null && defaultSite.Latitude != 0 && defaultSite.Longitude != 0)
            {
                var origin = new Coordinates { Latitude = defaultSite.Latitude, Longitude = defaultSite.Longitude };
                var destination = new Coordinates { Latitude = address.Latitude, Longitude = address.Longitude };

                // Intentar distancia por carretera; si falla, Haversine como fallback silencioso
                var distance = await geolocalizationService.GetDistanceAsync(origin, destination);
                if (distance.HasValue)
                {
                    logger.LogInformation("Distance (road) for address {AddressId}: {Distance} km", address.Id, distance.Value);
                }
                else
                {
                    logger.LogWarning("Road distance API unavailable for address {AddressId}, using Haversine fallback.", address.Id);
                }
                address.DistanceFromSite = distance ?? Coordinates.HaversineDistanceKm(origin, destination);
                logger.LogInformation("DistanceFromSite set for address {AddressId}: {Distance} km", address.Id, address.DistanceFromSite);
            }
            else
            {
                logger.LogWarning("No valid default site found; skipping distance calculation for address {AddressId}.", address.Id);
            }
        }
    }
}
