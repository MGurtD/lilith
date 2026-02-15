using Domain.Entities.Production;

namespace Application.Contracts;

public interface IShiftDetailService
{
    Task<ShiftDetail?> GetById(Guid id);
    Task<IEnumerable<ShiftDetail>> GetByShiftId(Guid shiftId);
    Task<IEnumerable<ShiftDetail>> GetAll();
    Task<GenericResponse> Create(ShiftDetail shiftDetail);
    Task<GenericResponse> Update(ShiftDetail shiftDetail);
    Task<GenericResponse> Remove(Guid id);
}