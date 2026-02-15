using Application.Contracts;
using Domain.Entities.Purchase;

namespace Application.Contracts;

public interface IExpenseService
{
    Task<Expenses?> GetExpenseById(Guid id);
    IEnumerable<Expenses> GetBetweenDatesAndType(DateTime startTime, DateTime endTime, Guid? expenseTypeId);
    IEnumerable<Expenses> GetByType(Guid expenseTypeId);
    IEnumerable<ConsolidatedExpense> GetConsolidatedBetweenDates(DateTime startTime, DateTime endTime);
    Task<GenericResponse> Create(Expenses expense);
    Task<GenericResponse> UpdateExpense(Expenses expense);
    Task<GenericResponse> RemoveExpense(Guid id);
}
