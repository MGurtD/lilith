using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkMasterService
{
    Task<WorkMaster?> GetById(Guid id);
    Task<WorkMaster?> GetByIdForCostCalculation(Guid id);
    Task<IEnumerable<WorkMaster>> GetAll();
    Task<IEnumerable<WorkMaster>> GetByReferenceId(Guid referenceId);
    Task<GenericResponse> Create(WorkMaster workMaster);
    Task<GenericResponse> Update(WorkMaster workMaster);
    Task<GenericResponse> Remove(Guid id);
    Task<GenericResponse> Copy(WorkMasterCopy request);
}
