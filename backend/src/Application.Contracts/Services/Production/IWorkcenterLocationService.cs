using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkcenterLocationService
{
    Task<WorkcenterLocation?> GetById(Guid id);
    Task<IEnumerable<WorkcenterLocation>> GetAll();
    Task<IEnumerable<WorkcenterLocation>> GetByWorkcenterId(Guid workcenterId);
    Task<GenericResponse> Create(WorkcenterLocation workcenterLocation);
    Task<GenericResponse> Update(WorkcenterLocation workcenterLocation);
    Task<GenericResponse> Remove(Guid id);
}
