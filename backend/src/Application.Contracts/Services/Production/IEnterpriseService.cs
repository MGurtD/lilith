using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IEnterpriseService
    {
        Task<Site?> GetDefaultSite();
        Task<Enterprise?> GetById(Guid id);
        Task<IEnumerable<Enterprise>> GetAll();
        Task<GenericResponse> Create(Enterprise enterprise);
        Task<GenericResponse> Update(Enterprise enterprise);
        Task<GenericResponse> Remove(Guid id);
    }
}
