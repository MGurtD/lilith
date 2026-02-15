using Application.Contracts;
using Domain.Entities.Auth;

namespace Application.Contracts;

public interface IMenuItemService
{
    Task<GenericResponse> GetAll(bool hierarchy = false);
    Task<GenericResponse> Get(Guid id);
    Task<GenericResponse> Create(MenuItem item);
    Task<GenericResponse> Update(MenuItem item);
    Task<GenericResponse> Delete(Guid id);
}
