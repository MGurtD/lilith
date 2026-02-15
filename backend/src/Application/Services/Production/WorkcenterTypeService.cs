using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class WorkcenterTypeService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IWorkcenterTypeService
    {
        public async Task<WorkcenterType?> GetById(Guid id)
        {
            return await unitOfWork.WorkcenterTypes.Get(id);
        }

        public async Task<IEnumerable<WorkcenterType>> GetAll()
        {
            var workcenterTypes = await unitOfWork.WorkcenterTypes.GetAll();
            return workcenterTypes.OrderBy(w => w.Name);
        }

        public async Task<GenericResponse> Create(WorkcenterType workcenterType)
        {
            var exists = unitOfWork.WorkcenterTypes.Find(w => w.Name == workcenterType.Name).Any();
            if (exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterTypeAlreadyExists", workcenterType.Name));
            }

            await unitOfWork.WorkcenterTypes.Add(workcenterType);
            return new GenericResponse(true, workcenterType);
        }

        public async Task<GenericResponse> Update(WorkcenterType workcenterType)
        {
            var exists = await unitOfWork.WorkcenterTypes.Exists(workcenterType.Id);
            if (!exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", workcenterType.Id));
            }

            await unitOfWork.WorkcenterTypes.Update(workcenterType);
            return new GenericResponse(true, workcenterType);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var workcenterType = unitOfWork.WorkcenterTypes.Find(w => w.Id == id).FirstOrDefault();
            if (workcenterType == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.WorkcenterTypes.Remove(workcenterType);
            return new GenericResponse(true, workcenterType);
        }
    }
}
