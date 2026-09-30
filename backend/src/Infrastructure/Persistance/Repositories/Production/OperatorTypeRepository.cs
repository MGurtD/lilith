using Application.Contracts;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.Repositories.Production;

public class OperatorTypeRepository(ApplicationDbContext context) : Repository<OperatorType, Guid>(context), IOperatorTypeRepository
{
}
