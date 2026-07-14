using Application.Contracts.Dtos;

namespace Application.Contracts.Services.Production
{
    /// <summary>
    /// Returns a lightweight branding payload for a given Enterprise.
    /// Used at app boot to load theme, colors, logos and sidebar title.
    /// </summary>
    public interface IBrandingService
    {
        Task<BrandingDto?> GetBrandingAsync(Guid enterpriseId);
        Task<GenericResponse> UpdateBrandingAsync(Guid enterpriseId, BrandingDto dto);
    }
}