using Domain.Entities.Transport;

namespace Application.Contracts
{
    public interface ITransportRateRepository : IRepository<TransportRate, Guid>
    {
        Task<IEnumerable<TransportRate>> GetCurrentTransportRatesBySupplierId(Guid supplierId);
        Task<IEnumerable<TransportRate>> GetTransportRatesWithDetailsBySupplierId(Guid supplierId);
    }
}