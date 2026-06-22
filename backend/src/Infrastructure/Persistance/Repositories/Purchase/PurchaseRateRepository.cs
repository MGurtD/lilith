using Application.Contracts.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseRateRepository(ApplicationDbContext context) : Repository<PurchaseRate, Guid>(context), IPurchaseRateRepository
    {
        public override async Task<PurchaseRate?> Get(Guid id)
        {
            return await dbSet
                        .Include(x => x.Details).ThenInclude(d => d.Reference)
                        .Include(x => x.Supplier)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<PurchaseRate>> GetByReferenceId(Guid referenceId)
        {
            return await dbSet
                        .Include(x => x.Supplier)
                        .Include(x => x.Details).ThenInclude(d => d.Reference)
                        .Where(x => x.Details.Any(d => d.ReferenceId == referenceId))
                        .AsSplitQuery()
                        .AsNoTracking()
                        .OrderByDescending(x => x.ValidFrom)
                        .ToListAsync();
        }
    }
}
