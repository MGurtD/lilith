using Application.Contracts;
using Domain.Entities.Sales;

namespace Application.Contracts
{
    public interface IBudgetService
    {
        Task<GenericResponse> Create(CreateHeaderRequest createRequest);

        Task<Budget?> GetById(Guid id);
        IEnumerable<Budget> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<Budget> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId);
        Task<GenericResponse> Update(Budget budget);
        Task<GenericResponse> Remove(Guid id);

        Task<GenericResponse> AddDetail(BudgetDetail detail);
        Task<GenericResponse> UpdateDetail(BudgetDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);
        Task<GenericResponse> RejectOutdatedBudgets();

    }
}
