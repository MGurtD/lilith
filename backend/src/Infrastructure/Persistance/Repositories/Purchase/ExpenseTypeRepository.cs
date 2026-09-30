using Application.Contracts;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase;

public class ExpenseTypeRepository(ApplicationDbContext context) : Repository<ExpenseType, Guid>(context), IExpenseTypeRepository
{
}
