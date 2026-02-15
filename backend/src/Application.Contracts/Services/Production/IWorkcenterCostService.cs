using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IWorkcenterCostService
    {
        Task<WorkcenterCost?> GetById(Guid id);
        Task<IEnumerable<WorkcenterCost>> GetAll();
        Task<WorkcenterCost?> GetByWorkcenterAndStatusId(Guid workcenterId, Guid statusId);
        Task<GenericResponse> Create(WorkcenterCost workcenterCost);
        Task<GenericResponse> Update(WorkcenterCost workcenterCost);
        Task<GenericResponse> Remove(Guid id);
    }
}
