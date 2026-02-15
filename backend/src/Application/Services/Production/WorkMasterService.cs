using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public class WorkMasterService(IUnitOfWork unitOfWork, IMetricsService metricsService, ILocalizationService localizationService) : IWorkMasterService
    {
        public async Task<WorkMaster?> GetById(Guid id)
        {
            return await unitOfWork.WorkMasters.Get(id);
        }

        public async Task<WorkMaster?> GetByIdForCostCalculation(Guid id)
        {
            return await unitOfWork.WorkMasters.Get(id);
        }

        public async Task<IEnumerable<WorkMaster>> GetAll()
        {
            var workMasters = await unitOfWork.WorkMasters.GetAll();
            return workMasters.OrderBy(w => w.ReferenceId);
        }

        public async Task<IEnumerable<WorkMaster>> GetByReferenceId(Guid referenceId)
        {
            var workMasters = unitOfWork.WorkMasters.Find(w => w.ReferenceId == referenceId && w.Disabled == false);
            return workMasters.OrderBy(w => w.BaseQuantity);
        }

        public async Task<GenericResponse> Create(WorkMaster workMaster)
        {
            var existsReference = await unitOfWork.References.Exists(workMaster.ReferenceId);
            if (!existsReference)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("ReferenceNotFound"));
            }

            var exists = unitOfWork.WorkMasters.Find(w => w.Id == workMaster.Id).Any();
            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("WorkMasterAlreadyExists"));
            }

            await unitOfWork.WorkMasters.Add(workMaster);
            return new GenericResponse(true, workMaster);
        }

        public async Task<GenericResponse> Update(WorkMaster workMaster)
        {
            var exists = await unitOfWork.WorkMasters.Exists(workMaster.Id);
            if (!exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", workMaster.Id));
            }

            // Calculate costs using metrics service
            var resultCosts = await metricsService.GetWorkmasterMetrics(workMaster, workMaster.BaseQuantity);
            if (resultCosts.Result && resultCosts.Content is ProductionMetrics workMasterMetrics)
            {
                workMaster.operatorCost = workMasterMetrics.OperatorCost;
                workMaster.machineCost = workMasterMetrics.MachineCost;
                workMaster.externalCost = workMasterMetrics.ExternalServiceCost + workMasterMetrics.ExternalTransportCost;
                workMaster.materialCost = workMasterMetrics.MaterialCost;
                workMaster.totalWeight = workMasterMetrics.TotalWeight;
            }

            await unitOfWork.WorkMasters.Update(workMaster);

            // Update reference WorkMasterCost
            var reference = await unitOfWork.References.Get(workMaster.ReferenceId);
            if (reference != null)
            {
                reference.WorkMasterCost = workMaster.operatorCost + workMaster.machineCost + 
                    workMaster.externalCost + workMaster.materialCost;
                await unitOfWork.References.Update(reference);
            }

            return new GenericResponse(true, workMaster);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var workMaster = unitOfWork.WorkMasters.Find(w => w.Id == id).FirstOrDefault();
            if (workMaster == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", id));
            }

            await unitOfWork.WorkMasters.Remove(workMaster);
            return new GenericResponse(true, workMaster);
        }

        public async Task<GenericResponse> Copy(WorkMasterCopy request)
        {
            // Validate reference if provided
            if (request.ReferenceId.HasValue && request.ReferenceId != Guid.Empty)
            {
                var exists = unitOfWork.WorkMasters.Find(w => 
                    w.ReferenceId == request.ReferenceId && 
                    w.Mode == request.Mode).Any();
                
                if (exists)
                {
                    return new GenericResponse(false,
                        localizationService.GetLocalizedString("ReferenceAlreadyExists"));
                }
            }

            // Execute copy operation
            var result = await unitOfWork.WorkMasters.Copy(request);
            if (result)
            {
                return new GenericResponse(true);
            }
            else
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("WorkMasterNotFound"));
            }
        }
    }
}
