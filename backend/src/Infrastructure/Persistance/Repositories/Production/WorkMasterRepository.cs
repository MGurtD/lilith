using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkMasterRepository : Repository<WorkMaster, Guid>, IWorkMasterRepository
    {
        public IWorkMasterPhaseRepository Phases { get; }

        public WorkMasterRepository(ApplicationDbContext context) : base(context)
        {
            Phases = new WorkMasterPhaseRepository(context);
        }

        public override async Task<IEnumerable<WorkMaster>> GetAll()
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .AsNoTracking()
                        .ToListAsync();
        }

        public override async Task<WorkMaster?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include(d => d.Phases)
                            .ThenInclude(d => d.Details)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<WorkMaster?> GetFullById(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include("Phases.Details")
                        .Include("Phases.BillOfMaterials")
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<WorkMaster>> GetByUpdatedOnFilter(DateTime? startDate, DateTime? endDate)
        {
            var query = dbSet.Include(d => d.Reference).AsNoTracking();

            if (startDate.HasValue)
                query = query.Where(w => w.UpdatedOn >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(w => w.UpdatedOn <= endDate.Value);

            return await query.ToListAsync();
        }
    }
}
