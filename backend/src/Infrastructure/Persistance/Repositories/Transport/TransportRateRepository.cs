using Domain.Entities.Transport;
using Microsoft.EntityFrameworkCore;
using Application.Contracts;

namespace Infrastructure.Persistance.Repositories.Transport
{
    public class TransportRateRepository: Repository<TransportRate, Guid>, ITransportRateRepository
    {
        public TransportRateRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<TransportRate>> GetCurrentTransportRatesBySupplierId(Guid supplierId)
        {
            return await dbSet
                .Include(tr => tr.Details)
                .Where(tr => tr.SupplierId == supplierId && tr.ValidFrom <= DateOnly.FromDateTime(DateTime.Now) && tr.ValidTo >= DateOnly.FromDateTime(DateTime.Now))
                .ToListAsync();
        }

        public async Task<IEnumerable<TransportRate>> GetTransportRatesWithDetailsBySupplierId(Guid supplierId)
        {
            return await dbSet
                .Include(tr => tr.Details)
                .Where(tr => tr.SupplierId == supplierId)
                .ToListAsync();
        }
    }
}