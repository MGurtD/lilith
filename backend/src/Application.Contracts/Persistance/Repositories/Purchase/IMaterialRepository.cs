using Domain.Entities.Purchase;
using Domain.Entities.Shared;

namespace Application.Contracts;

public interface IMaterialRepository : IRepository<Reference, Guid>
{
    IEnumerable<Reference> GetMaterials();

}
