using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts;

public interface IOperatorService
{
    Task<Operator?> GetById(Guid id);
    Task<IEnumerable<Operator>> GetAll();
    Task<GenericResponse> Create(Operator operatorEntity);
    Task<GenericResponse> Update(Operator operatorEntity);
    Task<GenericResponse> Remove(Guid id);
}
