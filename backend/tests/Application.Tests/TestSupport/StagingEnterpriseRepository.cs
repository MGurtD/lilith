using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Tests.TestSupport;

/// <summary>
/// <see cref="StagingRepository{TEntity}"/> for enterprises; no enterprise is
/// reported as having sites.
/// </summary>
public sealed class StagingEnterpriseRepository : StagingRepository<Enterprise>, IEnterpriseRepository
{
    public Task<bool> IsInUse(Guid enterpriseId) => Task.FromResult(false);
}
