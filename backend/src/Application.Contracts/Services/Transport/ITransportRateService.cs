using Domain.Entities.Transport;

namespace Application.Contracts;

public interface ITransportRateService
{
    Task<IEnumerable<TransportRate>> GetCurrentTransportRatesBySupplierId(Guid supplierId);
    Task<IEnumerable<TransportRate>> GetTransportRatesBySupplierId(Guid supplierId);
    Task<IEnumerable<TransportRate>> GetTransportRateByWeightAndDistance(Guid supplierId, double weight, double distance);
    Task<TransportRate?> GetTransportRateById(Guid id);
    Task<IEnumerable<TransportRate>> GetAllTransportRates();
    Task<GenericResponse> CreateTransportRate(TransportRate transportRate);
    Task<GenericResponse> UpdateTransportRate(Guid id, TransportRate transportRate);
    Task<GenericResponse> RemoveTransportRate(Guid id);

    Task<IEnumerable<TransportRateDetail>> GetTransportRateDetails(Guid transportRateId);
    Task<GenericResponse> CreateTransportRateDetail(TransportRateDetail detail);
    Task<GenericResponse> UpdateTransportRateDetail(TransportRateDetail detail);
    Task<GenericResponse> RemoveTransportRateDetail(Guid id);
}
