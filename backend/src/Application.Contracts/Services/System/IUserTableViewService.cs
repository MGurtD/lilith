using Application.Contracts;
using Domain.Entities.Auth;

namespace Application.Contracts;

public interface IUserTableViewService
{
    // Read operations - return entities directly
    Task<IEnumerable<UserTableView>> GetByUserAndPage(Guid userId, string page);
    Task<UserTableView?> GetById(Guid id);

    // Write operations - return GenericResponse
    Task<GenericResponse> Create(UserTableView userTableView);
    Task<GenericResponse> Update(Guid id, UserTableView userTableView);
    Task<GenericResponse> Delete(Guid id);
    Task<GenericResponse> SetDefault(Guid id, bool isDefault);

    /// <summary>
    /// Idempotent get-or-create for the per-user, per-page default view.
    /// Returns the existing default view if one exists; otherwise creates
    /// a new one with Name="Per defecte", IsDefault=true, ViewConfig='{"columns":[]}'.
    /// </summary>
    Task<GenericResponse> EnsureDefault(EnsureDefaultRequest request);
}