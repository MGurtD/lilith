using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkcenterProfitPercentageRepository : IRepository<WorkcenterProfitPercentage, Guid>
{
    Task<IEnumerable<WorkcenterProfitPercentage>> GetByWorkcenterId(Guid workcenterId);
}