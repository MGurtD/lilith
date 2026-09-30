using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IWorkMasterRepository : IRepository<WorkMaster, Guid>
    {
    IWorkMasterPhaseRepository Phases { get; }

    Task<WorkMaster?> GetFullById(Guid id);
    Task<IEnumerable<WorkMaster>> GetByUpdatedOnFilter(DateTime? startDate, DateTime? endDate);
    }
}
