using Domain.Entities.Production;

namespace Application.Contracts;

public interface ISiteRepository : IRepository<Site, Guid>
{
    /// <summary>
    /// True when an area, a warehouse, a sales order, a delivery note or a
    /// sales invoice uses the site. The enterprise default site does not count.
    /// </summary>
    Task<bool> IsInUse(Guid siteId);
}
