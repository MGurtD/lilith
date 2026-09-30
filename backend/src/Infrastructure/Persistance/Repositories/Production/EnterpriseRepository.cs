using Application.Contracts;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.Repositories.Production;

public class EnterpriseRepository(ApplicationDbContext context) : Repository<Enterprise, Guid>(context), IEnterpriseRepository
{
}
