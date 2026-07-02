using Application.Contracts;
using Domain.Entities.Sales;

namespace Application.Contracts
{
    public interface IBudgetService
    {
        Task<GenericResponse> Create(CreateHeaderRequest createRequest);
        Task<GenericResponse> Accept(Guid id);

        Task<Budget?> GetById(Guid id);
        IEnumerable<Budget> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<Budget> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId);
        Task<GenericResponse> Update(Budget budget);
        Task<GenericResponse> Remove(Guid id);

        Task<GenericResponse> AddDetail(BudgetDetail detail);
        Task<GenericResponse> UpdateDetail(BudgetDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);
        Task<GenericResponse> RejectOutdatedBudgets();

        Task<GenericResponse> AddTransport(BudgetTransport transport);
        Task<GenericResponse> UpdateTransport(BudgetTransport transport);
        Task<GenericResponse> RemoveTransport(Guid id);
        Task<GenericResponse> DistributeTransportCosts(Guid budgetId);
        Task<GenericResponse> DistributeAllCosts(Guid budgetId);
        Task<GenericResponse> UpdateExternalService(BudgetExternalServices externalService);
        Task<GenericResponse> Clone(Guid id, Guid newId);
    }
}
