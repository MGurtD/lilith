using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkcenterRepository : IRepository<Workcenter, Guid>
{
    Task<IEnumerable<Workcenter>> GetVisibleInPlant();
    Task<IEnumerable<WorkcenterLoadDto>> GetWorkcenterLoadBetweenDatesByWorkcenterType(DateTime startDate, DateTime endDate);
}