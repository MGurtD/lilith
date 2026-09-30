using Application.Contracts;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.Repositories.Production;

public class WorkcenterTypeRepository(ApplicationDbContext context) : Repository<WorkcenterType, Guid>(context), IWorkcenterTypeRepository
{
}
