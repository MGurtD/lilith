using Application.Contracts;
using Domain.Entities.Auth;

namespace Application.Contracts;

public interface IUserFilterService
{
    // Read operations - return entities directly
    Task<IEnumerable<UserFilter>> GetUserFiltersByUserId(Guid userId);
    Task<UserFilter?> GetUserFilterByUserIdPageKey(Guid userId, string page, string key);

    // Write operations - return GenericResponse
    Task<GenericResponse> CreateUserFilter(UserFilter userFilter);
    Task<GenericResponse> UpdateUserFilter(UserFilter userFilter);
    Task<GenericResponse> RemoveUserFilter(Guid id);
}
