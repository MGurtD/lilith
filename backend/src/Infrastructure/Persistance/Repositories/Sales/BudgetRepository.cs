using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class BudgetRepository : Repository<Budget, Guid>, IBudgetRepository
    {
        public IRepository<BudgetDetail, Guid> Details { get; }

        public BudgetRepository(ApplicationDbContext context) : base(context) 
        {
            Details = new Repository<BudgetDetail, Guid>(context);
        }

        public override async Task<Budget?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Details)
                            .ThenInclude(d => d.Reference)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
