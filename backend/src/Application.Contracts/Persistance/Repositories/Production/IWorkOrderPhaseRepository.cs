using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IWorkOrderPhaseRepository : IRepository<WorkOrderPhase, Guid>
    {
        IRepository<WorkOrderPhaseDetail, Guid> Details { get; }
        IRepository<WorkOrderPhaseBillOfMaterials, Guid> BillOfMaterials { get; }
    }
}
