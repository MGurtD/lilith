using Domain.Entities.Production;

namespace Application.Contracts;

public interface IAreaRepository : IRepository<Area, Guid>
{
    Task<IEnumerable<Area>> GetVisibleInPlantWithWorkcenters();
}
