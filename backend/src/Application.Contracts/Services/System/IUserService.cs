using Application.Contracts;
using Domain.Entities.Auth;

namespace Application.Contracts;

public interface IUserService
{
    // Read operations - return entities directly
    Task<IEnumerable<User>> GetAllUsers();
    Task<User?> GetUserById(Guid id);

    // Write operations - return GenericResponse
    Task<GenericResponse> CreateUser(User user);
    Task<GenericResponse> UpdateUser(User user);
}
