using Microsoft.AspNetCore.Http;

namespace Application.Contracts;

public interface IBrandingService
{
    Task<BrandingResponse> GetCurrent();
    Task<BrandingLogoContent?> GetCurrentLogo(BrandingLogoSlot slot);
    Task<GenericResponse> UpdateCurrent(BrandingUpdateRequest request);
    Task<GenericResponse> UploadCurrentLogo(BrandingLogoSlot slot, IFormFile? file);
    Task<GenericResponse> RemoveCurrentLogo(BrandingLogoSlot slot);
    Task<GenericResponse> UploadLogo(Guid enterpriseId, BrandingLogoSlot slot, IFormFile? file);
    Task<GenericResponse> RemoveLogo(Guid enterpriseId, BrandingLogoSlot slot);
    Task<GenericResponse> RemoveEnterpriseFiles(Guid enterpriseId);
}
