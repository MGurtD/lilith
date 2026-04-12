using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IWorkMasterRepository : IRepository<WorkMaster, Guid>
    {
        IWorkMasterPhaseRepository Phases { get; }

        Task<WorkMaster?> GetFullById(Guid id);
    }
}
