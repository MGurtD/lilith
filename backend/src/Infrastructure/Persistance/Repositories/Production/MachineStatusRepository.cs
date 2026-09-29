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

        // The shift history, production route phase steps and phase template steps
        // reference the machine status with a cascading foreign key, so deleting a
        // status in use would silently delete them; work order phase steps would
        // block the delete.
        public async Task<bool> IsInUse(Guid machineStatusId)
        {
            return await context.Set<WorkcenterShiftDetail>().AnyAsync(e => e.MachineStatusId == machineStatusId)
                || await context.Set<WorkMasterPhaseDetail>().AnyAsync(e => e.MachineStatusId == machineStatusId)
                || await context.Set<WorkOrderPhaseDetail>().AnyAsync(e => e.MachineStatusId == machineStatusId)
                || await context.Set<PhaseTemplateDetail>().AnyAsync(e => e.MachineStatusId == machineStatusId);
        }
    }
}
