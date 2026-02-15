using System.Linq.Expressions;
using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class ProductionPartRepository : Repository<ProductionPart, Guid>, IProductionPartRepository
    {

        public ProductionPartRepository(ApplicationDbContext context) : base(context)
        { }

        public override IEnumerable<ProductionPart> Find(Expression<Func<ProductionPart, bool>> predicate)
        {
            return dbSet
                    .Include(d => d.WorkOrder)
                    .Include(d => d.WorkOrderPhase)
                    .Include(d => d.WorkOrderPhaseDetail)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .Where(predicate);
        } 
    }
}
