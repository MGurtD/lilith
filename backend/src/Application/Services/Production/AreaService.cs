using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class AreaService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IAreaService
    {
        public async Task<Area?> GetById(Guid id)
        {
            return await unitOfWork.Areas.Get(id);
        }

        public async Task<IEnumerable<Area>> GetAll()
        {
            var areas = await unitOfWork.Areas.GetAll();
            return areas.OrderBy(a => a.Name);
        }

        public async Task<IEnumerable<Area>> GetVisibleInPlantWithWorkcenters()
        {
            return await unitOfWork.Areas.GetVisibleInPlantWithWorkcenters();
        }

        public async Task<GenericResponse> Create(Area area)
        {
            var exists = unitOfWork.Areas.Find(a => a.Name == area.Name).Any();
            if (exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("AreaAlreadyExists", area.Name));
            }

            await unitOfWork.Areas.Add(area);
            return new GenericResponse(true, area);
        }

        public async Task<GenericResponse> Update(Area area)
        {
            var exists = await unitOfWork.Areas.Exists(area.Id);
            if (!exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", area.Id));
            }

            await unitOfWork.Areas.Update(area);
            return new GenericResponse(true, area);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var area = unitOfWork.Areas.Find(a => a.Id == id).FirstOrDefault();
            if (area == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.Areas.Remove(area);
            return new GenericResponse(true, area);
        }
    }
}
