using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class EnterpriseService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IEnterpriseService
    {
        public async Task<Site?> GetDefaultSite()
        {
            var enterprise = (await unitOfWork.Enterprises.GetAll()).FirstOrDefault();
            if (enterprise == null || !enterprise.DefaultSiteId.HasValue)
                return null;

            return await unitOfWork.Sites.Get(enterprise.DefaultSiteId.Value);
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

            await unitOfWork.Enterprises.Add(enterprise);
            return new GenericResponse(true, enterprise);
        }

        public async Task<GenericResponse> Update(Enterprise enterprise)
        {
            var exists = await unitOfWork.Enterprises.Exists(enterprise.Id);
            if (!exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", enterprise.Id));
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

            await unitOfWork.Enterprises.Remove(enterprise);
            return new GenericResponse(true, enterprise);
        }
    }
}





