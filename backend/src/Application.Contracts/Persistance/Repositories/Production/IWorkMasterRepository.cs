using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IWorkMasterRepository : IRepository<WorkMaster, Guid>
    {
    IWorkMasterPhaseRepository Phases { get; }

    Task<WorkMaster?> GetFullById(Guid id);
    Task<IEnumerable<WorkMaster>> GetByUpdatedOnFilter(DateTime? startDate, DateTime? endDate);

    /// <summary>
    /// True when a work order, a budget line or a sales order line uses the
    /// production route. Its own phases do not count.
    /// </summary>
    Task<bool> IsInUse(Guid workMasterId);
    }
}
