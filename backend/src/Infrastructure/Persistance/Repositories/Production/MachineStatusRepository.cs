using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class MachineStatusRepository(ApplicationDbContext context) : Repository<MachineStatus, Guid>(context), IMachineStatusRepository
    {
        public IRepository<MachineStatusReason, Guid> Reasons { get; } = new Repository<MachineStatusReason, Guid>(context);

        public override async Task<MachineStatus?> Get(Guid id)
        {
            return await dbSet
                        .Include(m => m.Reasons)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<MachineStatus>> GetAllWithReasons()
        {
            return await dbSet
                        .Include(m => m.Reasons.Where(r => !r.Disabled))
                        .Where(m => !m.Disabled)
                        .AsNoTracking()
                        .ToListAsync();
        }
    }
}
