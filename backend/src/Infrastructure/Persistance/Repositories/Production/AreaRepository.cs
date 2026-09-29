using Application.Contracts;
using Domain.Entities.Production;
using Domain.Entities.Shared;
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

    // Workcenters reference the area with a cascading foreign key, and their
    // production parts and shift history cascade from them, so deleting an area in
    // use would silently delete all of them; references would block the delete.
    public async Task<bool> IsInUse(Guid areaId)
    {
        return await context.Set<Workcenter>().AnyAsync(w => w.AreaId == areaId)
            || await context.Set<Reference>().AnyAsync(r => r.AreaId == areaId);
    }
}
