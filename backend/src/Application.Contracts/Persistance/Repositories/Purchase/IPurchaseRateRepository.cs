using Domain.Entities.Purchase;

namespace Application.Contracts.Persistance.Repositories.Purchase
{
    public interface IPurchaseRateRepository : IRepository<PurchaseRate, Guid>
    {
    }
}
