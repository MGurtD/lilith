using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface ISiteService
    {
        Task<Site?> GetById(Guid id);
        Task<IEnumerable<Site>> GetAll();
        Task<GenericResponse> Create(Site site);
        Task<GenericResponse> Update(Site site);
        Task<GenericResponse> Remove(Guid id);
    }
}
