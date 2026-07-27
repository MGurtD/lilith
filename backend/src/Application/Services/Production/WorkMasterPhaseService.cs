using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production;

public class WorkMasterPhaseService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IWorkMasterPhaseService
{
    #region Phase CRUD
    
    public async Task<WorkMasterPhase?> GetById(Guid id)
    {
        return await unitOfWork.WorkMasters.Phases.Get(id);
    }

    public async Task<GenericResponse> Create(WorkMasterPhase phase)
    {
        // Validate ModelState is handled by controller
        
        // Check if phase already exists
        var exists = unitOfWork.WorkMasters.Phases.Find(p => p.Id == phase.Id).Any();
        if (exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkMasterPhaseAlreadyExists"));

        // Validate parent WorkMaster exists
        var workMaster = await unitOfWork.WorkMasters.Get(phase.WorkMasterId);
        if (workMaster is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkMasterNotFound", phase.WorkMasterId));

        // Create phase
        await unitOfWork.WorkMasters.Phases.Add(phase);
        return new GenericResponse(true, phase);
    }

    public async Task<GenericResponse> Update(WorkMasterPhase phase)
    {
        // Check if phase exists
        var exists = await unitOfWork.WorkMasters.Phases.Exists(phase.Id);
        if (!exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkMasterPhaseNotFound"));

        // Update phase
        await unitOfWork.WorkMasters.Phases.Update(phase);
        return new GenericResponse(true, phase);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.WorkMasters.Phases.Find(p => p.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkMasterPhaseNotFound"));

        await unitOfWork.WorkMasters.Phases.Remove(entity);
        return new GenericResponse(true, entity);
    }
    
    #endregion

    #region PhaseDetail CRUD
    
    public async Task<WorkMasterPhaseDetail?> GetDetailById(Guid id)
    {
        return await unitOfWork.WorkMasters.Phases.Details.Get(id);
    }

    public async Task<GenericResponse> CreateDetail(WorkMasterPhaseDetail detail)
    {
        // Check if detail already exists
        var exists = unitOfWork.WorkMasters.Phases.Details.Find(d => d.Id == detail.Id).Any();
        if (exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkMasterPhaseDetailAlreadyExists"));

        // Check if workcenter and machine status combination exists
        var phase = await unitOfWork.WorkMasters.Phases.Get(detail.WorkMasterPhaseId);
        var workcenters = await unitOfWork.Workcenters.FindAsync(w => w.WorkcenterTypeId == phase.WorkcenterTypeId);
        foreach (var w in workcenters)
        {
            var existsCost = unitOfWork.WorkcenterCosts.Find(wc => wc.WorkcenterId == w.Id && wc.MachineStatusId == detail.MachineStatusId).Any();
            if (!existsCost)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("WorkcenterCostNotFound", w.Id, detail.MachineStatusId));
            }
        }
        // Create detail
        await unitOfWork.WorkMasters.Phases.Details.Add(detail);
        return new GenericResponse(true, detail);
    }

    public async Task<GenericResponse> UpdateDetail(WorkMasterPhaseDetail detail)
    {
        // Check if detail exists
        var exists = await unitOfWork.WorkMasters.Phases.Details.Exists(detail.Id);
        if (!exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkMasterPhaseDetailNotFound"));
// Check if workcenter and machine status combination exists
        var phase = await unitOfWork.WorkMasters.Phases.Get(detail.WorkMasterPhaseId);
        var workcenters = await unitOfWork.Workcenters.FindAsync(w => w.WorkcenterTypeId == phase.WorkcenterTypeId);
        foreach (var w in workcenters)
        {
            var existsCost = unitOfWork.WorkcenterCosts.Find(wc => wc.WorkcenterId == w.Id && wc.MachineStatusId == detail.MachineStatusId).Any();
            if (!existsCost)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("WorkcenterCostNotFound", w.Id, detail.MachineStatusId));
            }
        }
        // Update detail
        await unitOfWork.WorkMasters.Phases.Details.Update(detail);
        return new GenericResponse(true, detail);
    }

    public async Task<GenericResponse> RemoveDetail(Guid id)
    {
        var entity = unitOfWork.WorkMasters.Phases.Details.Find(d => d.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkMasterPhaseDetailNotFound"));

        await unitOfWork.WorkMasters.Phases.Details.Remove(entity);
        return new GenericResponse(true, entity);
    }
    
    #endregion

    #region BillOfMaterials CRUD
    
    public async Task<WorkMasterPhaseBillOfMaterials?> GetBillOfMaterialsById(Guid id)
    {
        return await unitOfWork.WorkMasters.Phases.BillOfMaterials.Get(id);
    }

    public async Task<GenericResponse> CreateBillOfMaterials(WorkMasterPhaseBillOfMaterials item)
    {
        // Check if item already exists
        var exists = unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(b => b.Id == item.Id).Any();
        if (exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkMasterPhaseDetailAlreadyExists"));

        // Create item
        await unitOfWork.WorkMasters.Phases.BillOfMaterials.Add(item);
        return new GenericResponse(true, item);
    }

    public async Task<GenericResponse> UpdateBillOfMaterials(WorkMasterPhaseBillOfMaterials item)
    {
        // Check if item exists
        var exists = await unitOfWork.WorkMasters.Phases.BillOfMaterials.Exists(item.Id);
        if (!exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkMasterPhaseDetailNotFound"));

        // Update item
        await unitOfWork.WorkMasters.Phases.BillOfMaterials.Update(item);
        return new GenericResponse(true, item);
    }

    public async Task<GenericResponse> RemoveBillOfMaterials(Guid id)
    {
        var entity = unitOfWork.WorkMasters.Phases.BillOfMaterials.Find(b => b.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkMasterPhaseDetailNotFound"));

        await unitOfWork.WorkMasters.Phases.BillOfMaterials.Remove(entity);
        return new GenericResponse(true, entity);
    }
    
    #endregion
}
