using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production;

public class AreaRepository : Repository<Area, Guid>, IAreaRepository
{
    public AreaRepository(ApplicationDbContext context) : base(context) 
    { }

    public async Task<IEnumerable<Area>> GetVisibleInPlantWithWorkcenters()
    {
        return await dbSet
            .Include(a => a.Site)
            .Include(a => a.Workcenters!.Where(w => !w.Disabled))
                .ThenInclude(w => w.WorkcenterType)
            .Include(a => a.Workcenters!.Where(w => !w.Disabled))
                .ThenInclude(w => w.Shift)
            .Where(a => !a.Disabled && a.IsVisibleInPlant)
            .OrderBy(a => a.Name)
            .AsNoTracking()
            .ToListAsync();
    }
}
