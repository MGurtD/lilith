using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Application.Contracts;

namespace Infrastructure.Persistance.Repositories
{
    public class ContractReader<Entity> : IContractReader<Entity> where Entity : Application.Contracts.Contract
    {
        protected ApplicationDbContext context;
        internal DbSet<Entity> dbSet;

        public ContractReader(ApplicationDbContext context)
        {
            this.context = context;
            dbSet = context.Set<Entity>();
        }

        public virtual async Task<IEnumerable<Entity>> GetAll()
        {
            return await dbSet.ToListAsync();
        }

        public virtual IEnumerable<Entity> Find(Expression<Func<Entity, bool>> predicate)
        {
            return dbSet.Where(predicate);
        }

        public async Task<List<Entity>> FindAsync(Expression<Func<Entity, bool>> predicate)
        {
            return await dbSet.Where(predicate).ToListAsync();
        }
        
    }
}
