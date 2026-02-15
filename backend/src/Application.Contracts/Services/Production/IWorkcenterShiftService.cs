using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkcenterShiftService
{
    IWorkcenterShiftDetailService DetailsService { get; }

    Task<WorkcenterShift?> GetWorkcenterShift(Guid workcenterShiftId);
    Task<List<WorkcenterShift>> GetCurrentWorkcenterShifts();
    Task<List<WorkcenterShift>> GetCurrentsWithDetails();

    Task<GenericResponse> CreateWorkcenterShifts(List<CreateWorkcenterShiftDto> dtos);

    Task<GenericResponse> DisableWorkcenterShift(Guid workcenterShiftId);
    Task<List<WorkcenterShiftHistorical>> GetWorkcenterShiftHistorical(WorkcenterShiftHistoricRequest request);

}
