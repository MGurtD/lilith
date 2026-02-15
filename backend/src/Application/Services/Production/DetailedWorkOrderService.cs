using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production;

public class DetailedWorkOrderService(IUnitOfWork unitOfWork) : IDetailedWorkOrderService
{
    public IEnumerable<DetailedWorkOrder> GetByWorkcenterId(Guid workcenterId)
    {
        return unitOfWork.DetailedWorkOrders.Find(d => d.WorkcenterId == workcenterId);
    }

    public IEnumerable<DetailedWorkOrder> GetByWorkOrderId(Guid workOrderId)
    {
        return unitOfWork.DetailedWorkOrders.Find(d => d.WorkOrderId == workOrderId);
    }

    public IEnumerable<DetailedWorkOrder> GetByWorkOrderPhaseId(Guid workOrderPhaseId)
    {
        return unitOfWork.DetailedWorkOrders.Find(d => d.WorkOrderPhaseId == workOrderPhaseId);
    }
}
