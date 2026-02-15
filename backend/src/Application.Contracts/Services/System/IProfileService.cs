using Application.Contracts;
using Domain.Entities.Auth;

namespace Application.Contracts;

public interface IProfileService
{
    Task<GenericResponse> Create(Profile profile);
    Task<GenericResponse> Update(Profile profile);
    Task<GenericResponse> Delete(Guid id);
    Task<GenericResponse> GetAll();
    Task<GenericResponse> Get(Guid id);
    Task<GenericResponse> AssignMenu(Guid profileId, IEnumerable<Guid> menuItemIds, Guid? defaultMenuItemId = null);
    Task<GenericResponse> GetMenuForProfile(Guid profileId);
    Task<GenericResponse> GetMenuForUser(Guid userId);
    Task<GenericResponse> SetUserProfile(Guid userId, Guid profileId);
}
