using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class WorkcenterCostService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IWorkcenterCostService
    {
        public async Task<WorkcenterCost?> GetById(Guid id)
        {
            return await unitOfWork.WorkcenterCosts.Get(id);
        }

        public async Task<IEnumerable<WorkcenterCost>> GetAll()
        {
            var workcenterCosts = await unitOfWork.WorkcenterCosts.GetAll();
            return workcenterCosts;
        }

        public async Task<WorkcenterCost?> GetByWorkcenterAndStatusId(Guid workcenterId, Guid statusId)
        {
            var entities = await unitOfWork.WorkcenterCosts.FindAsync(w => 
                w.WorkcenterId == workcenterId && w.MachineStatusId == statusId);
            return entities.FirstOrDefault();
        }

        public async Task<GenericResponse> Create(WorkcenterCost workcenterCost)
        {
            // Check composite key uniqueness (WorkcenterId + MachineStatusId)
            var exists = unitOfWork.WorkcenterCosts.Find(w => 
                w.WorkcenterId == workcenterCost.WorkcenterId && 
                w.MachineStatusId == workcenterCost.MachineStatusId).Any();
            if (exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityAlreadyExists"));
            }

            await unitOfWork.WorkcenterCosts.Add(workcenterCost);
            return new GenericResponse(true, workcenterCost);
        }

        public async Task<GenericResponse> Update(WorkcenterCost workcenterCost)
        {
            var exists = await unitOfWork.WorkcenterCosts.Exists(workcenterCost.Id);
            if (!exists)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", workcenterCost.Id));
            }

            await unitOfWork.WorkcenterCosts.Update(workcenterCost);
            return new GenericResponse(true, workcenterCost);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var workcenterCost = unitOfWork.WorkcenterCosts.Find(w => w.Id == id).FirstOrDefault();
            if (workcenterCost == null)
            {
                return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.WorkcenterCosts.Remove(workcenterCost);
            return new GenericResponse(true, workcenterCost);
        }
    }
}
