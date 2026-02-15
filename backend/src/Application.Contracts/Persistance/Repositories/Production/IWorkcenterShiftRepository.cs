using Domain.Entities.Production;
using System.Linq.Expressions;

namespace Application.Contracts
{
    public interface IWorkcenterShiftRepository : IRepository<WorkcenterShift, Guid>
    {
        IWorkcenterShiftDetailRepository Details { get; }

        Task<WorkcenterShift?> GetCurrentWorkcenterShiftWithCurrentDetails(Guid workcenterId);
        
        Task<List<WorkcenterShift>> GetCurrentsWithDetails();

        IQueryable<WorkcenterShift> FindWithDetails(Expression<Func<WorkcenterShift, bool>> predicate);

        Task<List<WorkcenterShiftHistorical>> GetWorkcenterShiftHistorical(WorkcenterShiftHistoricRequest request);
    }
}
