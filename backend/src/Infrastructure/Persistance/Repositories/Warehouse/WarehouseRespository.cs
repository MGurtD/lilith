using Application.Contracts;
using Domain.Entities.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Warehouse
{
    public class WarehouseRepository(ApplicationDbContext context) : Repository<Domain.Entities.Warehouse.Warehouse, Guid>(context), IWarehouseRepository
    {
        public IRepository<Location, Guid> Locations { get; } = new Repository<Location, Guid>(context);

        public override async Task<Domain.Entities.Warehouse.Warehouse?> Get(Guid id)
        {
            return await dbSet
                        .Include(w => w.Locations)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetBySiteId(Guid siteId)
        {
            return await dbSet
                        .Where(w => w.SiteId == siteId)
                        .AsNoTracking()
                        .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetAllWithLocations()
        {
            return await dbSet
                        .Include(w => w.Locations)
                        .AsNoTracking()
                        .ToListAsync();
        }

        public async Task<Location?> GetDefaultLocation()
        {
            var warehouse = await dbSet.Include(w => w.Locations).FirstOrDefaultAsync(w => w.Disabled == false);
            if (warehouse == null || warehouse.Locations == null) return null;
            return warehouse.Locations.FirstOrDefault(l => l.Id == warehouse.DefaultLocationId);
        }

    }
}
