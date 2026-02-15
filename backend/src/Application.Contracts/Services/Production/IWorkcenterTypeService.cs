using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IWorkcenterTypeService
    {
        Task<WorkcenterType?> GetById(Guid id);
        Task<IEnumerable<WorkcenterType>> GetAll();
        Task<GenericResponse> Create(WorkcenterType workcenterType);
        Task<GenericResponse> Update(WorkcenterType workcenterType);
        Task<GenericResponse> Remove(Guid id);
    }
}
