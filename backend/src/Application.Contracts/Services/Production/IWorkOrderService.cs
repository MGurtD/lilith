using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkOrderService
{
    Task<WorkOrder?> GetById(Guid id);
    Task<GenericResponse> Create(WorkOrder workOrder);
    Task<GenericResponse> CreateFromWorkMaster(CreateWorkOrderDto dto);
    Task<GenericResponse> Start(Guid id);
    Task<GenericResponse> End(Guid id);
    Task<GenericResponse> Delete(Guid id); 
    IEnumerable<WorkOrder> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid? statusId);
    Task<IEnumerable<WorkOrderPhaseEstimationDto>>GetWorkcenterLoadBetweenDatesByWorkcenterType(DateTime startDate, DateTime endDate);
    IEnumerable<DetailedWorkOrder> GetWorkOrderDetails(Guid id);
    Task<IEnumerable<WorkOrder>> GetBySalesOrderId(Guid salesOrderId);
    Task<IEnumerable<WorkOrder>> GetPlannableWorkOrders();
    Task<GenericResponse> Priorize(List<UpdateWorkOrderOrderDTO> orders);
    Task Update(WorkOrder workOrder);
    Task<GenericResponse> UpdateStatusAfterPhaseEnd(Guid workOrderId, Guid completedPhaseId, Guid phaseOutStatusId);

    Task<GenericResponse> AddProductionPart(Guid id, ProductionPart productionPart);
    Task<GenericResponse> RemoveProductionPart(Guid id, ProductionPart productionPart);
    
}
