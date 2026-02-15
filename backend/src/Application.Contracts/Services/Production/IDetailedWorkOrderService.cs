using Domain.Entities.Production;

namespace Application.Contracts;

public interface IDetailedWorkOrderService
{
    IEnumerable<DetailedWorkOrder> GetByWorkcenterId(Guid workcenterId);
    IEnumerable<DetailedWorkOrder> GetByWorkOrderId(Guid workOrderId);
    IEnumerable<DetailedWorkOrder> GetByWorkOrderPhaseId(Guid workOrderPhaseId);
}
