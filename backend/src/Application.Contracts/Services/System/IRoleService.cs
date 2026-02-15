using Application.Contracts;
using Domain.Entities.Auth;

namespace Application.Contracts;

public interface IRoleService
{
    // Read operations - return entities directly
    Task<IEnumerable<Role>> GetAllRoles();
    Task<Role?> GetRoleById(Guid id);

    // Write operations - return GenericResponse
    Task<GenericResponse> CreateRole(Role role);
    Task<GenericResponse> UpdateRole(Role role);
    Task<GenericResponse> RemoveRole(Guid id);
}
