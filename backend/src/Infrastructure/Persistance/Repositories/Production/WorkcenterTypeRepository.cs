using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production;

public class WorkcenterTypeRepository(ApplicationDbContext context) : Repository<WorkcenterType, Guid>(context), IWorkcenterTypeRepository
{
    // Workcenters reference their type with a cascading foreign key, and their
    // production parts and shift history cascade from them, so deleting a type in
    // use would silently delete all of them; the phases would block the delete.
    public async Task<bool> IsInUse(Guid workcenterTypeId)
    {
        return await context.Set<Workcenter>().AnyAsync(w => w.WorkcenterTypeId == workcenterTypeId)
            || await context.Set<WorkMasterPhase>().AnyAsync(p => p.WorkcenterTypeId == workcenterTypeId)
            || await context.Set<WorkOrderPhase>().AnyAsync(p => p.WorkcenterTypeId == workcenterTypeId);
    }
}
