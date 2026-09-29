using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase;

public class ExpenseTypeRepository(ApplicationDbContext context) : Repository<ExpenseType, Guid>(context), IExpenseTypeRepository
{
    // Expenses reference their type with a cascading foreign key, so deleting a
    // type in use would silently delete them.
    public async Task<bool> IsInUse(Guid expenseTypeId)
    {
        return await context.Set<Expenses>().AnyAsync(e => e.ExpenseTypeId == expenseTypeId);
    }
}
