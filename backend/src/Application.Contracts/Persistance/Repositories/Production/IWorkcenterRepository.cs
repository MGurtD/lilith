using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkcenterRepository : IRepository<Workcenter, Guid>
{
    Task<IEnumerable<Workcenter>> GetVisibleInPlant();
    Task<IEnumerable<WorkcenterLoadDto>> GetWorkcenterLoadBetweenDatesByWorkcenterType(DateTime startDate, DateTime endDate);

    /// <summary>
    /// True when a production part, a shift history row or a production route or
    /// work order phase (as preferred workcenter) uses the workcenter. Costs,
    /// profit percentages and location links belong to the workcenter and do not count.
    /// </summary>
    Task<bool> IsInUse(Guid workcenterId);
}