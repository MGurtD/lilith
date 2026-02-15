using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkcenterShiftDetailService
{
    Task<WorkcenterShiftDetail?> GetWorkcenterShiftDetailById(Guid id);
    Task<List<WorkcenterShiftDetail>> GetCurrentWorkcenterShiftDetailsByWorkcenterShiftId(Guid workcenterShiftDetailId);

    Task<GenericResponse> OperatorIn(OperatorInOutRequest request);
    Task<GenericResponse> OperatorOut(OperatorInOutRequest request);
    Task<GenericResponse> WorkOrderPhaseIn(WorkOrderPhaseInRequest request);
    Task<GenericResponse> WorkOrderPhaseAndStatusIn(WorkOrderPhaseAndStatusInRequest request);
    Task<GenericResponse> WorkOrderPhaseOut(WorkOrderPhaseOutRequest request);
    Task<GenericResponse> ChangeWorkcenterStatus(WorkcenterChangeStatusRequest request);
    Task<GenericResponse> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto);

    Task<GenericResponse> DisableWorkcenterShiftDetail(Guid workcenterShiftDetailId);
}
