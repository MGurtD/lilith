using Domain.Entities.Purchase;

namespace Application.Contracts;

public interface IExpenseTypeService
{
    Task<IEnumerable<ExpenseType>> GetAllExpenseTypes();
    Task<ExpenseType?> GetExpenseTypeById(Guid id);
    Task<GenericResponse> CreateExpenseType(ExpenseType expenseType);
    Task<GenericResponse> UpdateExpenseType(ExpenseType expenseType);
    Task<GenericResponse> RemoveExpenseType(Guid id);
}
