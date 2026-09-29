using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production;

public class EnterpriseRepository(ApplicationDbContext context) : Repository<Enterprise, Guid>(context), IEnterpriseRepository
{
    // Sites reference the enterprise with a cascading foreign key, and areas,
    // warehouses and delivery notes cascade from them, so deleting an enterprise
    // with sites would silently delete all of them.
    public async Task<bool> IsInUse(Guid enterpriseId)
    {
        return await context.Set<Site>().AnyAsync(s => s.EnterpriseId == enterpriseId);
    }
}
