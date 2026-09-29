using Domain.Entities.Purchase;

namespace Application.Contracts;

public interface IExpenseTypeRepository : IRepository<ExpenseType, Guid>
{
    /// <summary>
    /// True when an expense uses the expense type.
    /// </summary>
    Task<bool> IsInUse(Guid expenseTypeId);
}
