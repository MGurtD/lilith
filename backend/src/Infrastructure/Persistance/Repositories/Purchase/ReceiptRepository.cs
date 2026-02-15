using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class ReceiptRepository(ApplicationDbContext context) : Repository<Receipt, Guid>(context), IReceiptRepository
    {
        public IRepository<ReceiptDetail, Guid> Details { get; } = new Repository<ReceiptDetail, Guid>(context);
        public IRepository<PurchaseOrderReceiptDetail, Guid> Receptions { get; } = new Repository<PurchaseOrderReceiptDetail, Guid>(context);

        public override async Task<Receipt?> Get(Guid id)
        {
            return await dbSet
                        .Include("Details.Reference")
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<PurchaseOrderReceiptDetail>> GetReceptions(Guid id)
        {
            var receipt = await Get(id);
            if (receipt == null)
                return [];

            var detailIds = receipt.Details!.Select(d => d.Id).ToList();
            var receptions = await Receptions.FindAsync(d => detailIds.Contains(d.ReceiptDetailId));

            foreach (var reception in receptions)
            {
                reception.ReceiptDetail = await Details.Get(reception.ReceiptDetailId);
                reception.ReceiptDetail!.Receipt = await Get(reception.ReceiptDetail.ReceiptId);
            }
            return receptions;
        }

        public async Task<List<Receipt>> GetReceiptsByReferenceId(Guid referenceId)
        {
            return await dbSet
                    .Include(r => r.Details)
                    .Where(r => r.Details.Any(d => d.ReferenceId == referenceId))
                    .AsSplitQuery() 
                    .AsNoTracking()
                    .ToListAsync();
        }
    }
}
