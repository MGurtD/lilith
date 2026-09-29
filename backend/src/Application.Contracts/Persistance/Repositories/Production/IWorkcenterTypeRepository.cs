using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkcenterTypeRepository : IRepository<WorkcenterType, Guid>
{
    /// <summary>
    /// True when a workcenter, a production route phase or a work order phase
    /// uses the workcenter type.
    /// </summary>
    Task<bool> IsInUse(Guid workcenterTypeId);
}
