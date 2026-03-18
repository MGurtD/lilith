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
    }
}
