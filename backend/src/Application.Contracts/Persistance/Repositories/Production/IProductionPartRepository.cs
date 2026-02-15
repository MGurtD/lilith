using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IProductionPartRepository : IRepository<ProductionPart, Guid>
    { }
}
