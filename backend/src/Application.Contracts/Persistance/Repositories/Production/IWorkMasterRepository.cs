using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IWorkMasterRepository : IRepository<WorkMaster, Guid>
    {
        Task<bool> Copy(WorkMasterCopy workMasterCopy);
        IWorkMasterPhaseRepository Phases { get; }

        Task<WorkMaster?> GetFullById(Guid id);
    }
}
