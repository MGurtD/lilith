using Application.Contracts;
using Domain.Entities;

namespace Application.Contracts;

public interface ITaxService
{
    Task<GenericResponse> CreateTax(Tax tax);
    Task<IEnumerable<Tax>> GetAllTaxes();
    Task<Tax?> GetTaxById(Guid id);
    Task<GenericResponse> UpdateTax(Tax tax);
    Task<GenericResponse> RemoveTax(Guid id);
}
