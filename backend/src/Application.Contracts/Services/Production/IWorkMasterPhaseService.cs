using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkMasterPhaseService
{
    // Phase CRUD
    Task<WorkMasterPhase?> GetById(Guid id);
    Task<GenericResponse> Create(WorkMasterPhase phase);
    Task<GenericResponse> Update(WorkMasterPhase phase);
    Task<GenericResponse> Remove(Guid id);
    
    // PhaseDetail CRUD
    Task<WorkMasterPhaseDetail?> GetDetailById(Guid id);
    Task<GenericResponse> CreateDetail(WorkMasterPhaseDetail detail);
    Task<GenericResponse> UpdateDetail(WorkMasterPhaseDetail detail);
    Task<GenericResponse> RemoveDetail(Guid id);
    
    // BillOfMaterials CRUD
    Task<WorkMasterPhaseBillOfMaterials?> GetBillOfMaterialsById(Guid id);
    Task<GenericResponse> CreateBillOfMaterials(WorkMasterPhaseBillOfMaterials item);
    Task<GenericResponse> UpdateBillOfMaterials(WorkMasterPhaseBillOfMaterials item);
    Task<GenericResponse> RemoveBillOfMaterials(Guid id);
}
