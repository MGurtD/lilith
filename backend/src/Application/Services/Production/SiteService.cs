using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class SiteService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ISiteService
    {
        public async Task<Site?> GetById(Guid id)
        {
            return await unitOfWork.Sites.Get(id);
        }

        public async Task<IEnumerable<Site>> GetAll()
        {
            var sites = await unitOfWork.Sites.GetAll();
            return sites.OrderBy(s => s.Name);
        }

        public async Task<GenericResponse> Create(Site site)
        {
            var exists = unitOfWork.Sites.Find(s => s.Name == site.Name).Any();
            if (exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("SiteAlreadyExists", site.Name));
            }

            await unitOfWork.Sites.Add(site);
            return new GenericResponse(true, site);
        }

        public async Task<GenericResponse> Update(Site site)
        {
            var exists = await unitOfWork.Sites.Exists(site.Id);
            if (!exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", site.Id));
            }

            await unitOfWork.Sites.Update(site);
            return new GenericResponse(true, site);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var site = unitOfWork.Sites.Find(s => s.Id == id).FirstOrDefault();
            if (site == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.Sites.Remove(site);
            return new GenericResponse(true, site);
        }
    }
}
