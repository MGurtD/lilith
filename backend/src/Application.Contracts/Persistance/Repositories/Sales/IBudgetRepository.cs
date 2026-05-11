using Domain.Entities.Sales;

namespace Application.Contracts
{
    public interface IBudgetRepository : IRepository<Budget, Guid>
    {
        IRepository<BudgetDetail, Guid> Details { get; }
        IRepository<BudgetTransport, Guid> Transports { get; }  
        IRepository<BudgetExternalServices, Guid> ExternalServices { get; }
        IRepository<BudgetExternalServiceDetail, Guid> ExternalServiceDetails { get; }
    
    }
}

