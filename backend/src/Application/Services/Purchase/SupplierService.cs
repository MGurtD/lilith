using Application.Contracts;
using Application.Contracts.Services.Geolocalization;
using Domain.Entities.Purchase;
using Microsoft.Extensions.Logging;

namespace Application.Services.Purchase;

public class SupplierService(
    IUnitOfWork unitOfWork, 
    ILocalizationService localizationService,
    IGeolocalizationService geolocalizationService,
    ILogger<SupplierService> logger) : ISupplierService
{
    // Supplier CRUD
    public async Task<Supplier?> GetSupplierById(Guid id)
    {
        return await unitOfWork.Suppliers.Get(id);
    }

    public async Task<IEnumerable<Supplier>> GetAllSuppliers()
    {
        var suppliers = await unitOfWork.Suppliers.GetAll();
        return suppliers.OrderBy(s => s.ComercialName);
    }

    public IEnumerable<Supplier> GetLogisticSuppliers()
    {
        var suppliers = unitOfWork.Suppliers.GetLogisticSuppliers();
        return suppliers.OrderBy(s => s.ComercialName);
    }

    public async Task<GenericResponse> CreateSupplier(Supplier supplier)
    {
        var exists = unitOfWork.Suppliers.Find(r => supplier.ComercialName == r.ComercialName).Any();
        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("SupplierAlreadyExists", supplier.ComercialName));
        }

        await UpdateCoordinatesAndDistanceAsync(supplier);

        await unitOfWork.Suppliers.Add(supplier);
        return new GenericResponse(true, supplier);
    }

    public async Task<GenericResponse> UpdateSupplier(Supplier supplier)
    {
        var exists = await unitOfWork.Suppliers.Exists(supplier.Id);
        if (!exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", supplier.Id));
        }

        await UpdateCoordinatesAndDistanceAsync(supplier);

        await unitOfWork.Suppliers.Update(supplier);
        return new GenericResponse(true, supplier);
    }

    public async Task<GenericResponse> RemoveSupplier(Guid id)
    {
        var entity = unitOfWork.Suppliers.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.Suppliers.Remove(entity);
        return new GenericResponse(true, entity);
    }

    // Contact operations
    public async Task<GenericResponse> CreateContact(SupplierContact contact)
    {
        var supplier = await unitOfWork.Suppliers.Get(contact.SupplierId);
        if (supplier is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("SupplierNotFound", contact.SupplierId));
        }

        await unitOfWork.Suppliers.AddContact(contact);
        return new GenericResponse(true, contact);
    }

    public async Task<GenericResponse> UpdateContact(Guid id, SupplierContact contact)
    {
        var existing = unitOfWork.Suppliers.GetContactById(id);
        if (existing is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        existing.FirstName = contact.FirstName;
        existing.LastName = contact.LastName;
        existing.Email = contact.Email;
        existing.Phone = contact.Phone;
        existing.PhoneExtension = contact.PhoneExtension;
        existing.Charge = contact.Charge;
        existing.Disabled = contact.Disabled;
        existing.Default = contact.Default;
        existing.Observations = contact.Observations;

        await unitOfWork.Suppliers.UpdateContact(existing);
        return new GenericResponse(true, existing);
    }

    public async Task<GenericResponse> RemoveContact(Guid id)
    {
        var contact = unitOfWork.Suppliers.GetContactById(id);
        if (contact is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.Suppliers.RemoveContact(contact);
        return new GenericResponse(true, contact);
    }

    // SupplierReference operations
    public async Task<SupplierReference?> GetSupplierReferenceBySupplierIdAndReferenceId(Guid supplierId, Guid referenceId)
    {
        return await unitOfWork.Suppliers.GetSupplierReferenceBySupplierIdAndReferenceId(supplierId, referenceId);
    }

    public async Task<SupplierReference?> GetSupplierReferenceById(Guid supplierReferenceId)
    {
        return await unitOfWork.Suppliers.GetSupplierReferenceById(supplierReferenceId);
    }

    public IEnumerable<SupplierReference> GetSupplierReferences(Guid supplierId)
    {
        return unitOfWork.Suppliers.GetSupplierReferences(supplierId);
    }

    public IEnumerable<Supplier> GetSuppliersByReference(Guid referenceId)
    {
        return unitOfWork.Suppliers.GetReferenceSuppliers(referenceId);
    }

    public async Task<GenericResponse> CreateSupplierReference(SupplierReference supplierReference)
    {
        var supplier = await unitOfWork.Suppliers.Get(supplierReference.SupplierId);
        if (supplier is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("SupplierNotFound", supplierReference.SupplierId));
        }

        var reference = await unitOfWork.References.Get(supplierReference.ReferenceId);
        if (reference is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("ReferenceNotFound", supplierReference.ReferenceId));
        }

        var references = unitOfWork.Suppliers.GetSupplierReferences(supplierReference.SupplierId);
        var exists = references.Where(r => r.ReferenceId == supplierReference.ReferenceId).Any();
        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("SupplierReferenceAlreadyExists"));
        }

        await unitOfWork.Suppliers.AddSupplierReference(supplierReference);
        return new GenericResponse(true, supplierReference);
    }

    public async Task<GenericResponse> UpdateSupplierReference(Guid id, SupplierReference supplierReference)
    {
        var existing = await unitOfWork.Suppliers.GetSupplierReferenceById(id);
        if (existing is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        existing.UpdatedOn = DateTime.Now;
        existing.SupplierCode = supplierReference.SupplierCode;
        existing.SupplierDescription = supplierReference.SupplierDescription;
        existing.SupplierPrice = supplierReference.SupplierPrice;
        existing.SupplyDays = supplierReference.SupplyDays;

        await unitOfWork.Suppliers.UpdateSupplierReference(existing);
        return new GenericResponse(true, existing);
    }

    public async Task<GenericResponse> RemoveSupplierReference(Guid id)
    {
        var supplierReference = await unitOfWork.Suppliers.GetSupplierReferenceById(id);
        if (supplierReference is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.Suppliers.RemoveSupplierReference(supplierReference);
        return new GenericResponse(true, supplierReference);
    }

    private async Task UpdateCoordinatesAndDistanceAsync(Supplier supplier)
    {
        logger.LogInformation("UpdateCoordinatesAndDistanceAsync -> Supplier: {SupplierId} | Lat: {Lat}, Lon: {Lon}",
            supplier.Id, supplier.Latitude, supplier.Longitude);

        // 1. Si el frontend ya proporcionó coordenadas (vía AutocompleteLocation), confiar en ellas.
        //    Si no, intentar geocodificar como fallback.
        if (supplier.Latitude == 0 && supplier.Longitude == 0)
        {
            if (!string.IsNullOrWhiteSpace(supplier.Address) &&
                !string.IsNullOrWhiteSpace(supplier.City) &&
                !string.IsNullOrWhiteSpace(supplier.Country))
            {
                logger.LogInformation("Geocoding supplier {SupplierId}: {Address}, {City}, {Country}",
                    supplier.Id, supplier.Address, supplier.City, supplier.Country);

                var coords = await geolocalizationService.GetCoordinatesAsync(
                    supplier.Address, supplier.City, supplier.PostalCode, supplier.Country);
                if (coords != null)
                {
                    supplier.Latitude = coords.Latitude;
                    supplier.Longitude = coords.Longitude;
                    logger.LogInformation("Geocoding supplier {SupplierId} resolved -> Lat: {Lat}, Lon: {Lon}",
                        supplier.Id, coords.Latitude, coords.Longitude);
                }
                else
                {
                    logger.LogWarning("Geocoding supplier {SupplierId} returned no results.", supplier.Id);
                }
            }
            else
            {
                logger.LogWarning("Supplier {SupplierId} has no coordinates and insufficient address data to geocode.", supplier.Id);
            }
        }

        // 2. Si tenemos coordenadas válidas, calcular distancia desde el site por defecto
        if (supplier.Latitude != 0 && supplier.Longitude != 0)
        {
            var defaultSite = (await unitOfWork.Sites.GetAll()).FirstOrDefault();
            if (defaultSite != null && defaultSite.Latitude != 0 && defaultSite.Longitude != 0)
            {
                var origin = new Coordinates { Latitude = defaultSite.Latitude, Longitude = defaultSite.Longitude };
                var destination = new Coordinates { Latitude = supplier.Latitude, Longitude = supplier.Longitude };

                // Intentar distancia por carretera; si falla, Haversine como fallback silencioso
                var distance = await geolocalizationService.GetDistanceAsync(origin, destination);
                if (distance.HasValue)
                {
                    logger.LogInformation("Distance (road) for supplier {SupplierId}: {Distance} km", supplier.Id, distance.Value);
                }
                else
                {
                    logger.LogWarning("Road distance API unavailable for supplier {SupplierId}, using Haversine fallback.", supplier.Id);
                }
                supplier.DistanceFromSite = distance ?? Coordinates.HaversineDistanceKm(origin, destination);
                logger.LogInformation("DistanceFromSite set for supplier {SupplierId}: {Distance} km", supplier.Id, supplier.DistanceFromSite);
            }
            else
            {
                logger.LogWarning("No valid default site found; skipping distance calculation for supplier {SupplierId}.", supplier.Id);
            }
        }
    }
}
