using Domain.Entities.Production;

namespace Application.Contracts;

public interface IEnterpriseRepository : IRepository<Enterprise, Guid>
{
    /// <summary>
    /// True when the enterprise has sites.
    /// </summary>
    Task<bool> IsInUse(Guid enterpriseId);
}
