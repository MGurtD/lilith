using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IOperatorTypeService
    {
        Task<OperatorType?> GetById(Guid id);
        Task<IEnumerable<OperatorType>> GetAll();
        Task<GenericResponse> Create(OperatorType operatorType);
        Task<GenericResponse> Update(OperatorType operatorType);
        Task<GenericResponse> Remove(Guid id);
    }
}
