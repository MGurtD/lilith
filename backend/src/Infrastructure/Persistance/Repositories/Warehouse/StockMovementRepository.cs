using Application.Contracts;
using Domain.Entities.Warehouse;
using Infrastructure.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Warehouse
{
    public class StockMovementRepository(ApplicationDbContext context) : Repository<StockMovement, Guid>(context), IStockMovementRepository
    {
        public IEnumerable<StockMovement> GetBetweenDatesWithLocation(DateTime startDate, DateTime endDate, Guid? locationId)
        {
            var query = dbSet
                .Include(sm => sm.Location)
                .AsNoTracking()
                .Where(sm => sm.MovementDate >= startDate && sm.MovementDate <= endDate);

            if (locationId.HasValue)
            {
                query = query.Where(sm => sm.LocationId == locationId);
            }

            return query.ToList();
        }

        public IEnumerable<StockMovement> GetByEntityReferences(Guid workOrderId, IEnumerable<Guid> workOrderPhaseIds)
        {
            var phaseIdList = workOrderPhaseIds.ToList();

            return dbSet
                .Include(sm => sm.Location)
                .Include(sm => sm.Reference)
                .AsNoTracking()
                .Where(sm =>
                    (sm.Entity == StockMovementEntities.WorkOrder && sm.EntityId == workOrderId)
                    || (sm.Entity == StockMovementEntities.WorkOrderPhase && sm.EntityId.HasValue && phaseIdList.Contains(sm.EntityId.Value)))
                .OrderByDescending(sm => sm.MovementDate)
                .ToList();
        }

        public async Task<List<StockMovement>> GetByLotIds(IEnumerable<Guid> lotIds)
        {
            var lotIdList = lotIds.ToList();
            if (lotIdList.Count == 0)
                return [];

            return await dbSet
                .Include(sm => sm.Location)
                .AsNoTracking()
                .Where(sm => sm.LotId.HasValue && lotIdList.Contains(sm.LotId.Value))
                .OrderBy(sm => sm.MovementDate)
                .ToListAsync();
        }
    }
}
