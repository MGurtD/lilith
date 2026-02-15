using Application.Contracts;
using Domain.Entities.Shared;

namespace Application.Contracts;

public interface IReferenceTypeService
{
    Task<GenericResponse> CreateReferenceType(ReferenceType referenceType);
    Task<IEnumerable<ReferenceType>> GetAllReferenceTypes();
    Task<ReferenceType?> GetReferenceTypeById(Guid id);
    Task<GenericResponse> UpdateReferenceType(ReferenceType referenceType);
    Task<GenericResponse> RemoveReferenceType(Guid id);
}
