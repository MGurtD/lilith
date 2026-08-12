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

        public async Task<IEnumerable<StockListItemResponse>> GetStockList(Guid? locationId, Guid? referenceId)
        {
            var query = from st in context.Set<Stock>()
                        join r in context.Set<Reference>()
                            on st.ReferenceId equals r.Id
                        join l in context.Set<Location>()
                            on st.LocationId equals l.Id
                        join w in context.Set<Domain.Entities.Warehouse.Warehouse>()
                            on l.WarehouseId equals w.Id
                        join lot in context.Set<Lot>()
                            on st.LotId equals lot.Id into lotGroup
                        from lot in lotGroup.DefaultIfEmpty()
                        where st.Quantity > 0
                            && !r.Disabled
                            && !l.Disabled
                            && !w.Disabled
                            && (!locationId.HasValue || st.LocationId == locationId.Value)
                            && (!referenceId.HasValue || st.ReferenceId == referenceId.Value)
                        group st by new
                        {
                            st.ReferenceId,
                            ReferenceCode = r.Code,
                            ReferenceDescription = r.Description,
                            st.LocationId,
                            LocationName = l.Name,
                            LocationDescription = l.Description,
                            WarehouseId = w.Id,
                            WarehouseName = w.Name,
                            WarehouseDescription = w.Description,
                            st.Width,
                            st.Length,
                            st.Height,
                            st.Diameter,
                            st.Thickness,
                            st.LotId,
                            LotCode = lot != null ? lot.Code : "",
                            LotClosedDate = lot != null ? lot.ClosedDate : (DateTime?)null
                        } into stockGroup
                        select new StockListItemResponse
                        {
                            Id = stockGroup
                                .OrderBy(s => s.Id)
                                .Select(s => s.Id)
                                .FirstOrDefault(),
                            ReferenceId = stockGroup.Key.ReferenceId,
                            ReferenceCode = stockGroup.Key.ReferenceCode,
                            ReferenceDescription = stockGroup.Key.ReferenceDescription,
                            ReferenceDisplay = stockGroup.Key.ReferenceCode + " - " + stockGroup.Key.ReferenceDescription,
                            LocationId = stockGroup.Key.LocationId,
                            LocationName = stockGroup.Key.LocationName,
                            LocationDescription = stockGroup.Key.LocationDescription,
                            WarehouseId = stockGroup.Key.WarehouseId,
                            WarehouseName = stockGroup.Key.WarehouseName,
                            WarehouseDescription = stockGroup.Key.WarehouseDescription,
                            Quantity = stockGroup.Sum(s => s.Quantity),
                            Width = stockGroup.Key.Width,
                            Length = stockGroup.Key.Length,
                            Height = stockGroup.Key.Height,
                            Diameter = stockGroup.Key.Diameter,
                            Thickness = stockGroup.Key.Thickness,
                            LotId = stockGroup.Key.LotId,
                            LotCode = stockGroup.Key.LotCode,
                            LotClosedDate = stockGroup.Key.LotClosedDate
                        };

            return await query.AsNoTracking().ToListAsync();
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
                        join lot in context.Set<Lot>()
                            on st.LotId equals lot.Id into lotGroup
                        from lot in lotGroup.DefaultIfEmpty()
                        where r.Id == referenceId
                            && !r.Disabled
                            && (rf == null || !rf.Disabled)
                            && !l.Disabled
                            && !w.Disabled
                            && st.Quantity > 0
                        select new StockResponse
                        {
                            StockId = st.Id,
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
                            Thickness = st.Thickness,
                            LotId = st.LotId,
                            LotCreatedOn = lot != null ? lot.CreatedOn : (DateTime?)null
                        };

            return await query.ToListAsync();
        }

    }
}
