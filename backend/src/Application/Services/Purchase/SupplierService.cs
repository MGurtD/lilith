using Application.Contracts;
using Domain.Entities.Purchase;

namespace Application.Services.Purchase;

public class SupplierService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ISupplierService
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

    public async Task<GenericResponse> CreateSupplier(Supplier supplier)
    {
        var exists = unitOfWork.Suppliers.Find(r => supplier.ComercialName == r.ComercialName).Any();
        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("SupplierAlreadyExists", supplier.ComercialName));
        }

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
}
