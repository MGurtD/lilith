using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IAreaService
    {
        Task<Area?> GetById(Guid id);
        Task<IEnumerable<Area>> GetAll();
        Task<IEnumerable<Area>> GetVisibleInPlantWithWorkcenters();
        Task<GenericResponse> Create(Area area);
        Task<GenericResponse> Update(Area area);
        Task<GenericResponse> Remove(Guid id);
    }
}
