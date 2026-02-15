using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production;

public class WorkcenterProfitPercentageRepository : Repository<WorkcenterProfitPercentage, Guid>, IWorkcenterProfitPercentageRepository
{
    public WorkcenterProfitPercentageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkcenterProfitPercentage>> GetByWorkcenterId(Guid workcenterId)
    {
        return await dbSet.Where(w => w.WorkcenterId == workcenterId).ToListAsync();
    }
}