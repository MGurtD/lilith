using Domain.Entities;
using Domain.Entities.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IExpenseRepository : IRepository<Expenses, Guid>
    {
        //IEnumerable<Expenses> Find(Expression<Func<Expenses, bool>> predicate);
    }
}
