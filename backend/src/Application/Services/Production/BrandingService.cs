using Application.Contracts;
using Application.Contracts.Services.Production;
using Application.Contracts.Dtos;
using Application.Utils;

namespace Application.Services.Production
{
    /// <summary>
    /// Reads and writes branding fields on an Enterprise.
    /// Reads are unvalidated (the frontend may need to display whatever is on disk);
    /// writes run through <see cref="BrandingValidator"/> and return a non-success
    /// <see cref="GenericResponse"/> with a localized message when validation fails
    /// or the enterprise does not exist (the message will contain "not found" so
    /// controllers can map it to HTTP 404 — see LifecycleController convention).
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

            // Length enforcement mirrors the EF column widths configured in
            // EnterpriseBuilder. Without this, oversized input would reach EF and
            // fail with a database exception instead of a controlled validation
            // response.
            if (dto.Theme is { Length: > BrandingValidator.MaxLengthTheme })
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("BrandingThemeTooLong",
                        BrandingValidator.MaxLengthTheme));
            }
            if (dto.TitleSidebar is { Length: > BrandingValidator.MaxLengthTitleSidebar })
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("BrandingTitleTooLong",
                        BrandingValidator.MaxLengthTitleSidebar));
            }
            if (dto.LogoMain is { Length: > BrandingValidator.MaxLengthLogoUrl })
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("BrandingLogoUrlTooLong",
                        BrandingValidator.MaxLengthLogoUrl));
            }
            if (dto.LogoSidebar is { Length: > BrandingValidator.MaxLengthLogoUrl })
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("BrandingLogoUrlTooLong",
                        BrandingValidator.MaxLengthLogoUrl));
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