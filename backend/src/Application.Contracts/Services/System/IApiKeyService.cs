using Domain.Entities.Auth;

namespace Application.Contracts;

public interface IApiKeyService
{
    Task<IEnumerable<ApiKey>> GetAll();
    Task<ApiKey?> Get(Guid id);
    Task<GenericResponse> Create(CreateApiKeyRequest request);
    Task<GenericResponse> Disable(Guid id);
}
