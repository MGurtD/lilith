using Domain.Entities.Production;

namespace Application.Contracts;

public interface IAreaRepository : IRepository<Area, Guid>
{
    Task<IEnumerable<Area>> GetVisibleInPlantWithWorkcenters();

    /// <summary>
    /// True when a workcenter or a reference uses the area.
    /// </summary>
    Task<bool> IsInUse(Guid areaId);
}
