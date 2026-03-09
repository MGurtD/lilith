using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class PhaseTemplateRepository : Repository<PhaseTemplate, Guid>, IPhaseTemplateRepository
    {
        public IRepository<PhaseTemplateDetail, Guid> Details { get; }

        public PhaseTemplateRepository(ApplicationDbContext context) : base(context)
        {
            Details = new Repository<PhaseTemplateDetail, Guid>(context);
        }

        public override async Task<PhaseTemplate?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Details)
                            .ThenInclude(d => d.MachineStatus)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public override async Task<IEnumerable<PhaseTemplate>> GetAll()
        {
            return await dbSet
                        .Include(d => d.Details)
                            .ThenInclude(d => d.MachineStatus)
                        .AsNoTracking()
                        .ToListAsync();
        }
    }
}
