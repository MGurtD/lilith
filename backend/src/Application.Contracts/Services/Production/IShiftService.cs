using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IShiftService
    {
        Task<Shift?> GetById(Guid id);
        Task<IEnumerable<Shift>> GetAll();
        Task<GenericResponse> Create(Shift shift);
        Task<GenericResponse> Update(Shift shift);
        Task<GenericResponse> Remove(Guid id);
    }
}
