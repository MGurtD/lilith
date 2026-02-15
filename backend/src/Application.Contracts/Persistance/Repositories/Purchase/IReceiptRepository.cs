using Domain.Entities.Purchase;

namespace Application.Contracts
{
    public interface IReceiptRepository : IRepository<Receipt, Guid>
    {
        IRepository<ReceiptDetail, Guid> Details { get; }
        IRepository<PurchaseOrderReceiptDetail, Guid> Receptions { get; }

        Task<List<Receipt>> GetReceiptsByReferenceId(Guid referenceId);
        Task<List<PurchaseOrderReceiptDetail>> GetReceptions(Guid id);
    }
}
