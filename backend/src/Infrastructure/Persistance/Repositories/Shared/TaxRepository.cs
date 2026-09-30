using Application.Contracts;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories;

public class TaxRepository(ApplicationDbContext context) : Repository<Tax, Guid>(context), ITaxRepository
{
}
