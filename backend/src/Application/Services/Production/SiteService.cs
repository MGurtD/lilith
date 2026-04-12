using Application.Contracts;
using Application.Contracts.Services.Geolocalization;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class SiteService(
        IUnitOfWork unitOfWork, 
        ILocalizationService localizationService,
        IGeolocalizationService geolocalizationService) : ISiteService
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

            await UpdateCoordinatesAsync(site);

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

            await UpdateCoordinatesAsync(site);

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

        private async Task UpdateCoordinatesAsync(Site site)
        {
            if (string.IsNullOrWhiteSpace(site.Address) || string.IsNullOrWhiteSpace(site.City) || string.IsNullOrWhiteSpace(site.Country))
                return;

            var coords = await geolocalizationService.GetCoordinatesAsync(site.Address, site.City, site.PostalCode, site.Country);
            if (coords != null)
            {
                site.Latitude = coords.Latitude;
                site.Longitude = coords.Longitude;
            }
        }
    }
}
