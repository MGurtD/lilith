using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class EnterpriseService(
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IBrandingService brandingService) : IEnterpriseService
    {
        public async Task<Site?> GetDefaultSite()
        {
            var enabledEnterprises = await unitOfWork.Enterprises.FindAsync(e => !e.Disabled);
            if (enabledEnterprises.Count != 1)
                return null;

            var defaultSiteId = enabledEnterprises[0].DefaultSiteId;
            return defaultSiteId.HasValue
                ? await unitOfWork.Sites.Get(defaultSiteId.Value)
                : null;
        }

        public async Task<Enterprise?> GetById(Guid id)
        {
            return await unitOfWork.Enterprises.Get(id);
        }

        public async Task<IEnumerable<Enterprise>> GetAll()
        {
            var enterprises = await unitOfWork.Enterprises.GetAll();
            return enterprises.OrderBy(e => e.Name);
        }

        public async Task<GenericResponse> Create(Enterprise enterprise)
        {
            var exists = unitOfWork.Enterprises.Find(e => e.Name == enterprise.Name).Any();
            if (exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EnterpriseAlreadyExists", enterprise.Name));
            }

            if (!enterprise.Disabled && unitOfWork.Enterprises.Find(e => !e.Disabled).Any())
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EnterpriseActiveAlreadyExists"));
            }

            await unitOfWork.Enterprises.Add(enterprise);
            return new GenericResponse(true, enterprise);
        }

        public async Task<GenericResponse> Update(Enterprise enterprise)
        {
            var existing = await unitOfWork.Enterprises.Get(enterprise.Id);
            if (existing is null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", enterprise.Id));
            }

            // Branding is owned by the dedicated Branding API, not by this general update boundary.
            enterprise.BrandName = existing.BrandName;
            enterprise.PrimaryColor = existing.PrimaryColor;
            enterprise.LogoMainFileId = existing.LogoMainFileId;
            enterprise.LogoSidebarFileId = existing.LogoSidebarFileId;

            if (!enterprise.Disabled && unitOfWork.Enterprises.Find(e => !e.Disabled && e.Id != enterprise.Id).Any())
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EnterpriseActiveAlreadyExists"));
            }

            await unitOfWork.Enterprises.Update(enterprise);
            return new GenericResponse(true, enterprise);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var enterprise = unitOfWork.Enterprises.Find(e => e.Id == id).FirstOrDefault();
            if (enterprise == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", id));
            }

            var brandingCleanup = await brandingService.RemoveEnterpriseFiles(id);
            if (!brandingCleanup.Result)
                return brandingCleanup;

            await unitOfWork.Enterprises.Remove(enterprise);
            return new GenericResponse(true, enterprise);
        }
    }
}





