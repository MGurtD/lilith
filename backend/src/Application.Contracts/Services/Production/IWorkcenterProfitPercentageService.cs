using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkcenterProfitPercentageService
{
    Task<WorkcenterProfitPercentage?> GetById(Guid id);
    Task<IEnumerable<WorkcenterProfitPercentage>> GetAll();
    Task<IEnumerable<WorkcenterProfitPercentage>> GetByWorkcenterId(Guid workcenterId);
    Task<GenericResponse> Create(WorkcenterProfitPercentage workcenterProfitPercentage);
    Task<GenericResponse> Update(WorkcenterProfitPercentage workcenterProfitPercentage);
    Task<GenericResponse> Remove(Guid id);
}