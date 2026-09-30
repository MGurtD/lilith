using Domain.Entities.Purchase;

namespace Application.Contracts;

public interface IExpenseTypeRepository : IRepository<ExpenseType, Guid>
{
}
