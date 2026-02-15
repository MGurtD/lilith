using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class ExpenseRepository : Repository<Expenses, Guid>, IExpenseRepository
    {
        public ExpenseRepository(ApplicationDbContext context) : base(context) {}

        public virtual IEnumerable<Entity> Find(Expression<Func<Entity, bool>> predicate)
        {
            return dbSet.AsNoTracking().Where(predicate);
        }
    }
}
