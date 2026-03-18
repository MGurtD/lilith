using Application.Contracts;
using Domain.Entities.Auth;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services.System
{
    public class ApiKeyService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IApiKeyService
    {
        public async Task<IEnumerable<ApiKey>> GetAll()
        {
            var keys = await unitOfWork.ApiKeys.GetAll();
            return keys.OrderBy(k => k.Name);
        }

        public async Task<ApiKey?> Get(Guid id)
        {
            return await unitOfWork.ApiKeys.Get(id);
        }

        public async Task<GenericResponse> Create(CreateApiKeyRequest request)
        {
            var exists = unitOfWork.ApiKeys.Find(k => k.Name == request.Name).Any();
            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityAlreadyExists"));
            }

            var plainKey = GenerateApiKey();
            var keyPrefix = plainKey.Split('_')[1];
            var keyHash = HashApiKey(plainKey);

            var entity = new ApiKey
            {
                Name = request.Name,
                Description = request.Description,
                Scopes = request.Scopes,
                ExpiresOn = request.ExpiresOn,
                KeyPrefix = keyPrefix,
                KeyHash = keyHash,
                Disabled = false,
            };

            await unitOfWork.ApiKeys.Add(entity);

            return new GenericResponse(true, new CreateApiKeyResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                KeyPrefix = entity.KeyPrefix,
                ApiKey = plainKey,
                ExpiresOn = entity.ExpiresOn,
            });
        }

        public async Task<GenericResponse> Disable(Guid id)
        {
            var key = await unitOfWork.ApiKeys.Get(id);
            if (key is null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", id));
            }

            key.Disabled = true;
            await unitOfWork.ApiKeys.Update(key);

            return new GenericResponse(true, key);
        }

        private static string GenerateApiKey()
        {
            var prefixBytes = RandomNumberGenerator.GetBytes(6);
            var secretBytes = RandomNumberGenerator.GetBytes(32);

            var prefix = Convert.ToHexString(prefixBytes).ToLowerInvariant();
            var secret = Convert.ToBase64String(secretBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            return $"rs_{prefix}_{secret}";
        }

        private static string HashApiKey(string apiKey)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
