using Domain.Entities.Production;

namespace Application.Contracts;

public interface IMachineStatusRepository : IRepository<MachineStatus, Guid>
{
    IRepository<MachineStatusReason, Guid> Reasons { get; }
    Task<IEnumerable<MachineStatus>> GetAllWithReasons();

    /// <summary>
    /// True when a shift history row, a production route or work order phase step
    /// or a phase template step uses the machine status. Its reasons and
    /// workcenter costs belong to it and do not count.
    /// </summary>
    Task<bool> IsInUse(Guid machineStatusId);
}
