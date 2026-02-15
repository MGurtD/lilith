using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkMasterPhaseRepository : Repository<WorkMasterPhase, Guid>, IWorkMasterPhaseRepository
    {
        public IRepository<WorkMasterPhaseDetail, Guid> Details { get; }
        public IRepository<WorkMasterPhaseBillOfMaterials, Guid> BillOfMaterials { get; }

        public WorkMasterPhaseRepository(ApplicationDbContext context) : base(context)
        {
            Details = new Repository<WorkMasterPhaseDetail, Guid>(context);
            BillOfMaterials = new Repository<WorkMasterPhaseBillOfMaterials, Guid>(context);
        }

        public override async Task<WorkMasterPhase?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Details)
                        .Include(d => d.BillOfMaterials)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
