using Application.Contracts;
using Domain.Entities.Shared;

namespace Infrastructure.Persistance.Repositories;

public class ReferenceRepository(ApplicationDbContext context) : Repository<Reference, Guid>(context), IReferenceRepository
{
}
