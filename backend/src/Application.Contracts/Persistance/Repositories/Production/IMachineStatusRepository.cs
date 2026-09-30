using Domain.Entities.Production;

namespace Application.Contracts;

public interface IMachineStatusRepository : IRepository<MachineStatus, Guid>
{
    IRepository<MachineStatusReason, Guid> Reasons { get; }
    Task<IEnumerable<MachineStatus>> GetAllWithReasons();
}
