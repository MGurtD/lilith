using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkOrderPhaseRepository : Repository<WorkOrderPhase, Guid>, IWorkOrderPhaseRepository
    {
        public IRepository<WorkOrderPhaseDetail, Guid> Details { get; }
        public IRepository<WorkOrderPhaseBillOfMaterials, Guid> BillOfMaterials { get; }

        public WorkOrderPhaseRepository(ApplicationDbContext context) : base(context)
        {
            Details = new Repository<WorkOrderPhaseDetail, Guid>(context);
            BillOfMaterials = new Repository<WorkOrderPhaseBillOfMaterials, Guid>(context);
        }

        public override async Task<WorkOrderPhase?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Details)
                            .ThenInclude(d => d.MachineStatus)
                        .Include(d => d.BillOfMaterials)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
