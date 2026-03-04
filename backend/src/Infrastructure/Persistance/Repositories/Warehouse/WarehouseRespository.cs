using Application.Contracts;
using Domain.Entities.Purchase;
using Domain.Entities.Shared;
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

        public async Task<IEnumerable<StockResponse>> GetStockByReferenceId(Guid referenceId)
        {
            var query = from st in context.Set<Stock>()
                        join r in context.Set<Reference>() 
                            on st.ReferenceId equals r.Id
                        join rf in context.Set<ReferenceFormat>() 
                            on r.ReferenceFormatId equals rf.Id into rfGroup
                        from rf in rfGroup.DefaultIfEmpty()
                        join l in context.Set<Location>() 
                            on st.LocationId equals l.Id
                        join w in context.Set<Domain.Entities.Warehouse.Warehouse>() 
                            on l.WarehouseId equals w.Id
                        where r.Id == referenceId
                            && !r.Disabled
                            && (rf == null || !rf.Disabled)
                            && !l.Disabled
                            && !w.Disabled
                            && st.Quantity > 0
                        select new StockResponse
                        {
                            ReferenceId = r.Id,
                            ReferenceCode = r.Code,
                            ReferenceDescription = r.Description,
                            ReferenceFormatId = rf != null ? rf.Id : Guid.Empty,
                            ReferenceFormatCode = rf != null ? rf.Code : "",
                            ReferenceFormatDescription = rf != null ? rf.Description ?? "" : "",
                            LocationId = l.Id,
                            LocationName = l.Name,
                            LocationDescription = l.Description,
                            WarehouseId = w.Id,
                            WarehouseName = w.Name,
                            WarehouseDescription = w.Description,
                            Quantity = st.Quantity,
                            Width = st.Width,
                            Length = st.Length,
                            Height = st.Height,
                            Diameter = st.Diameter,
                            Thickness = st.Thickness
                        };

            return await query.ToListAsync();
        }

    }
}
