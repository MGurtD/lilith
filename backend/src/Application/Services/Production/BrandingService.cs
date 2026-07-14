using Application.Contracts;
using Application.Contracts.Services.Production;
using Application.Contracts.Dtos;
using Application.Utils;

namespace Application.Services.Production
{
    /// <summary>
    /// Reads branding fields from an Enterprise and projects them into a BrandingDto.
    /// Does not perform validation — invalid values stored on disk are returned as-is
    /// so the frontend can show them; the controller layer is responsible for any
    /// gatekeeping (currently it just 404s on missing enterprise).
    /// </summary>
    public class BrandingService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IBrandingService
    {
        public async Task<BrandingDto?> GetBrandingAsync(Guid enterpriseId)
        {
            var enterprise = await unitOfWork.Enterprises.Get(enterpriseId);
            if (enterprise is null)
            {
                return null;
            }

            return new BrandingDto
            {
                Theme = enterprise.Theme,
                PrimaryColor = enterprise.PrimaryColor,
                LogoMain = enterprise.LogoMain,
                LogoSidebar = enterprise.LogoSidebar,
                TitleSidebar = enterprise.TitleSidebar,
            };
        }

        public async Task<GenericResponse> UpdateBrandingAsync(Guid enterpriseId, BrandingDto dto)
        {
            var enterprise = await unitOfWork.Enterprises.Get(enterpriseId);
            if (enterprise is null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", enterpriseId));
            }

            if (!BrandingValidator.IsValidPrimaryColor(dto.PrimaryColor))
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("BrandingInvalidPrimaryColor"));
            }

            if (!BrandingValidator.IsValidLogoUrl(dto.LogoMain) ||
                !BrandingValidator.IsValidLogoUrl(dto.LogoSidebar))
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("BrandingInvalidLogoUrl"));
            }

            enterprise.Theme = dto.Theme;
            enterprise.PrimaryColor = dto.PrimaryColor;
            enterprise.LogoMain = dto.LogoMain;
            enterprise.LogoSidebar = dto.LogoSidebar;
            enterprise.TitleSidebar = dto.TitleSidebar;

            await unitOfWork.Enterprises.Update(enterprise);
            return new GenericResponse(true, dto);
        }
    }
}