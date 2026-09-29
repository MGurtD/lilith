using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production;

public class OperatorTypeRepository(ApplicationDbContext context) : Repository<OperatorType, Guid>(context), IOperatorTypeRepository
{
    // Operators reference their type with a cascading foreign key, so deleting a
    // type in use would silently delete them; the phases would block the delete.
    public async Task<bool> IsInUse(Guid operatorTypeId)
    {
        return await context.Set<Operator>().AnyAsync(o => o.OperatorTypeId == operatorTypeId)
            || await context.Set<WorkMasterPhase>().AnyAsync(p => p.OperatorTypeId == operatorTypeId)
            || await context.Set<WorkOrderPhase>().AnyAsync(p => p.OperatorTypeId == operatorTypeId);
    }
}
