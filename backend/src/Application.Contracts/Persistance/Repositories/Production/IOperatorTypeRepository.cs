using Domain.Entities.Production;

namespace Application.Contracts;

public interface IOperatorTypeRepository : IRepository<OperatorType, Guid>
{
    /// <summary>
    /// True when an operator, a production route phase or a work order phase
    /// uses the operator type.
    /// </summary>
    Task<bool> IsInUse(Guid operatorTypeId);
}
