using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseOrderRepository : Repository<PurchaseOrder, Guid>, IPurchaseOrderRepository
    {
        public IRepository<PurchaseOrderDetail, Guid> Details { get; }

        public IRepository<PurchaseOrderReceiptDetail, Guid> Receptions { get; }

        public PurchaseOrderRepository(ApplicationDbContext context) : base(context) 
        {
            Details = new Repository<PurchaseOrderDetail, Guid>(context);
            Receptions = new Repository<PurchaseOrderReceiptDetail, Guid>(context);
        }

        public override async Task<PurchaseOrder?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Details)
                            .ThenInclude(d => d.Reference)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
            
        }

        public async Task<PurchaseOrder?> GetHeaders(Guid purchaseOrderId)
        {
            return await dbSet.Include(d => d.Details).AsNoTracking().FirstOrDefaultAsync(e => e.Id == purchaseOrderId);
        }


        public async Task<List<PurchaseOrderReceiptDetail>> GetReceptions(Guid id)
        {
            var order = await Get(id);
            if (order == null)
                return [];

            var detailIds = order.Details.Select(d => d.Id).ToList();
            var receptions = await Receptions.FindAsync(d => detailIds.Contains(d.PurchaseOrderDetailId));

            var receiptRepository = new ReceiptRepository(context);
            foreach (var reception in receptions)
            {
                reception.ReceiptDetail = await receiptRepository.Details.Get(reception.ReceiptDetailId);
                reception.ReceiptDetail!.Receipt = await receiptRepository.dbSet.FirstOrDefaultAsync(r => r.Id == reception.ReceiptDetail.ReceiptId);
            }

            return receptions;
        }

        public async Task<List<PurchaseOrder>> GetOrdersWithDetailsToReceiptBySupplier(Guid supplierId, List<Guid> discartedStatuses)
        {
            return
                await dbSet
                    .Include(d => d.Details.Where(d => d.Quantity > d.ReceivedQuantity))
                        .ThenInclude(d => d.Reference)
                    .Include(d => d.Details)
                        .ThenInclude(d => d.WorkOrderPhase!.WorkOrder)
                .Where(e => 
                    e.SupplierId == supplierId && 
                    e.Details.Any(d => !discartedStatuses.Contains(d.StatusId)))
                .ToListAsync();
        }

        public async Task<List<PurchaseOrderDetail>> GetOrderDetailsFromReceptions(List<Guid> ids)
        {
            return await Details.FindAsync(d => ids.Contains(d.Id));
        }

    }
}
