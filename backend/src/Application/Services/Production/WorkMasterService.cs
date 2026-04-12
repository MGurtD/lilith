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
            // 1. Load the source WorkMaster with all related data
            var source = await unitOfWork.WorkMasters.GetFullById(request.WorkmasterId);
            if (source == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("WorkMasterNotFound"));
            }

            // 2. Determine target reference
            Guid targetReferenceId;

            if (request.ReferenceId.HasValue && request.ReferenceId != Guid.Empty)
            {
                // Copy to existing reference — check no WorkMaster with same Mode already exists
                var exists = unitOfWork.WorkMasters.Find(w =>
                    w.ReferenceId == request.ReferenceId.Value &&
                    w.Mode == request.Mode).Any();

                if (exists)
                {
                    return new GenericResponse(false,
                        localizationService.GetLocalizedString("ReferenceAlreadyExists"));
                }

                targetReferenceId = request.ReferenceId.Value;
            }
            else
            {
                // Create new reference copying all fields from the source reference
                var sourceReference = await unitOfWork.References.Get(source.ReferenceId);
                if (sourceReference == null)
                {
                    return new GenericResponse(false,
                        localizationService.GetLocalizedString("ReferenceNotFound"));
                }

                var newReference = new Domain.Entities.Shared.Reference
                {
                    Id = Guid.NewGuid(),
                    Code = request.ReferenceCode,
                    Description = !string.IsNullOrWhiteSpace(request.ReferenceDescription)
                        ? request.ReferenceDescription
                        : sourceReference.Description,
                    Version = sourceReference.Version,
                    TaxId = sourceReference.TaxId,
                    ReferenceTypeId = sourceReference.ReferenceTypeId,
                    CustomerId = sourceReference.CustomerId,
                    CategoryName = sourceReference.CategoryName,
                    AreaId = sourceReference.AreaId,
                    Cost = sourceReference.Cost,
                    Price = sourceReference.Price,
                    TransportAmount = sourceReference.TransportAmount,
                    Sales = sourceReference.Sales,
                    Purchase = sourceReference.Purchase,
                    Production = sourceReference.Production,
                    IsService = sourceReference.IsService,
                    ReferenceFormatId = sourceReference.ReferenceFormatId,
                    LastCost = sourceReference.LastCost,
                    WorkMasterCost = sourceReference.WorkMasterCost,
                    Disabled = false
                };

                await unitOfWork.References.AddWithoutSave(newReference);
                targetReferenceId = newReference.Id;
            }

            // 3. Create the new WorkMaster
            var newWorkMaster = new WorkMaster
            {
                Id = Guid.NewGuid(),
                ReferenceId = targetReferenceId,
                BaseQuantity = source.BaseQuantity,
                operatorCost = source.operatorCost,
                machineCost = source.machineCost,
                externalCost = source.externalCost,
                materialCost = source.materialCost,
                totalWeight = source.totalWeight,
                Mode = request.Mode,
                Disabled = false
            };

            await unitOfWork.WorkMasters.AddWithoutSave(newWorkMaster);

            // 4. Copy phases, details, and bill of materials
            foreach (var sourcePhase in source.Phases)
            {
                var newPhase = new WorkMasterPhase
                {
                    Id = Guid.NewGuid(),
                    WorkMasterId = newWorkMaster.Id,
                    Code = sourcePhase.Code,
                    Description = sourcePhase.Description,
                    OperatorTypeId = sourcePhase.OperatorTypeId,
                    WorkcenterTypeId = sourcePhase.WorkcenterTypeId,
                    ProfitPercentage = sourcePhase.ProfitPercentage,
                    PreferredWorkcenterId = sourcePhase.PreferredWorkcenterId,
                    IsExternalWork = sourcePhase.IsExternalWork,
                    ServiceReferenceId = sourcePhase.ServiceReferenceId,
                    ExternalWorkCost = sourcePhase.ExternalWorkCost,
                    TransportCost = sourcePhase.TransportCost,
                    Comment = sourcePhase.Comment,
                    Disabled = false
                };

                await unitOfWork.WorkMasters.Phases.AddWithoutSave(newPhase);

                // Copy phase details
                foreach (var sourceDetail in sourcePhase.Details)
                {
                    var newDetail = new WorkMasterPhaseDetail
                    {
                        Id = Guid.NewGuid(),
                        WorkMasterPhaseId = newPhase.Id,
                        MachineStatusId = sourceDetail.MachineStatusId,
                        Order = sourceDetail.Order,
                        IsCycleTime = sourceDetail.IsCycleTime,
                        EstimatedTime = sourceDetail.EstimatedTime,
                        EstimatedOperatorTime = sourceDetail.EstimatedOperatorTime,
                        Comment = sourceDetail.Comment,
                        Disabled = false
                    };

                    await unitOfWork.WorkMasters.Phases.Details.AddWithoutSave(newDetail);
                }

                // Copy phase bill of materials
                foreach (var sourceBom in sourcePhase.BillOfMaterials)
                {
                    var newBom = new WorkMasterPhaseBillOfMaterials
                    {
                        Id = Guid.NewGuid(),
                        WorkMasterPhaseId = newPhase.Id,
                        ReferenceId = sourceBom.ReferenceId,
                        Quantity = sourceBom.Quantity,
                        Width = sourceBom.Width,
                        Length = sourceBom.Length,
                        Height = sourceBom.Height,
                        Diameter = sourceBom.Diameter,
                        Thickness = sourceBom.Thickness,
                        Disabled = false
                    };

                    await unitOfWork.WorkMasters.Phases.BillOfMaterials.AddWithoutSave(newBom);
                }
            }

            // 5. Single SaveChanges for the entire operation (transactional)
            await unitOfWork.WorkMasters.SaveChanges();

            return new GenericResponse(true);
        }
    }
}
