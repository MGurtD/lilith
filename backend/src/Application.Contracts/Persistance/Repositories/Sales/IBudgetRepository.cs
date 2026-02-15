using Domain.Entities.Sales;

namespace Application.Contracts
{
    public interface IBudgetRepository : IRepository<Budget, Guid>
    {
        IRepository<BudgetDetail, Guid> Details { get; }
    }
}

