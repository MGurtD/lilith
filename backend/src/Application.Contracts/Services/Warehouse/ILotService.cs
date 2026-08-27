using Application.Contracts;
using Domain.Entities.Warehouse;

namespace Application.Contracts;

public interface ILotService
{
    Task<GenericResponse> Create(Lot lot);
    Task<IEnumerable<Lot>> GetAll();
    Task<Lot?> GetById(Guid id);
    Task<GenericResponse> Update(Lot lot);
    Task<GenericResponse> Remove(Guid id);

    Task<IEnumerable<Lot>> GetOpenLotsByReference(Guid referenceId);

    // Reutilitza el lot obert amb el mateix codi per a la referencia o en crea un de nou (mai reobre un lot tancat)
    Task<GenericResponse> ResolveOrCreateLot(Guid referenceId, string? code, string? supplierLotCode, DateTime? expirationDate);
}
