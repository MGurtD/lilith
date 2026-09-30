using Application.Contracts;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.Repositories.Production;

public class SiteRepository(ApplicationDbContext context) : Repository<Site, Guid>(context), ISiteRepository
{
}
