using Application.Contracts;
using Domain.Entities.Production;
using Domain.Entities.Warehouse;

namespace Application.Contracts;

public interface IWorkcenterService
{
    Task<Workcenter?> GetById(Guid id);
    Task<IEnumerable<Workcenter>> GetAll();
    Task<IEnumerable<Workcenter>> GetVisibleInPlant();
    Task<IEnumerable<WorkcenterLoadDto>> GetWorkcenterLoadBetweenDatesByWorkcenterType(DateTime startDate, DateTime endDate);
    Task<GenericResponse> Create(Workcenter workcenter);
    Task<GenericResponse> Update(Workcenter workcenter);
    Task<GenericResponse> Remove(Guid id);
    Task<IEnumerable<Location>> GetLocationsByType(Guid workcenterId, string locationType);
}
