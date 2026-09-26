using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class BudgetRepository : Repository<Budget, Guid>, IBudgetRepository
    {
        public IRepository<BudgetDetail, Guid> Details { get; }
        public IRepository<BudgetTransport, Guid> Transports { get; }
        public IRepository<BudgetExternalServices, Guid> ExternalServices { get; }
        public IRepository<BudgetExternalServiceDetail, Guid> ExternalServiceDetails { get; }
        public IRepository<BudgetDetailPhaseProfit, Guid> DetailPhaseProfits { get; }

        public BudgetRepository(ApplicationDbContext context) : base(context) 
        {
            Details = new Repository<BudgetDetail, Guid>(context);
            Transports = new Repository<BudgetTransport, Guid>(context);
            ExternalServices = new Repository<BudgetExternalServices, Guid>(context);
            ExternalServiceDetails = new Repository<BudgetExternalServiceDetail, Guid>(context);
            DetailPhaseProfits = new Repository<BudgetDetailPhaseProfit, Guid>(context);
        }

        public override async Task<Budget?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Details)
                            .ThenInclude(d => d.Reference)
                        .Include(d => d.Details)
                            .ThenInclude(d => d.PhaseProfits)
                        .Include(d => d.Transports)
                        .Include(d => d.ExternalServices)
                            .ThenInclude(es => es.Details)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
