using Application.Contracts;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production;

public class SiteRepository(ApplicationDbContext context) : Repository<Site, Guid>(context), ISiteRepository
{
    // Areas, warehouses and delivery notes reference the site with a cascading
    // foreign key, so deleting a site in use would silently delete them and
    // everything that cascades from them. The enterprise default site is set to
    // null and does not count.
    public async Task<bool> IsInUse(Guid siteId)
    {
        return await context.Set<Area>().AnyAsync(a => a.SiteId == siteId)
            || await context.Set<Domain.Entities.Warehouse.Warehouse>().AnyAsync(w => w.SiteId == siteId)
            || await context.Set<DeliveryNote>().AnyAsync(d => d.SiteId == siteId)
            || await context.Set<SalesInvoice>().AnyAsync(i => i.SiteId == siteId)
            || await context.Set<SalesOrderHeader>().AnyAsync(o => o.SiteId == siteId);
    }
}
