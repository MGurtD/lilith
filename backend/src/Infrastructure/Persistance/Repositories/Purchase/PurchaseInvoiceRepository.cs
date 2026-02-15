using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceRepository : Repository<PurchaseInvoice, Guid>, IPurchaseInvoiceRepository
    {
        public IRepository<PurchaseInvoiceImport, Guid> ImportsRepository { get; }

        public PurchaseInvoiceRepository(ApplicationDbContext context) : base(context)
        {
            ImportsRepository = new Repository<PurchaseInvoiceImport, Guid>(context);
        }

        public override async Task<PurchaseInvoice?> Get(Guid id)
        {
            return await dbSet
                .AsNoTracking()
                .Include(d => d.PurchaseInvoiceDueDates)
                .Include(d => d.PurchaseInvoiceImports)                
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public override IEnumerable<PurchaseInvoice> Find(Expression<Func<PurchaseInvoice, bool>> predicate)
        {
            return dbSet
                .AsNoTracking()
                .Include(d => d.PurchaseInvoiceDueDates)
                .Include(d => d.PurchaseInvoiceImports)
                .Where(predicate)
                .OrderBy(pi => pi.PurchaseInvoiceDate);
        }

        /*public int GetNextNumber()
        {
            if (dbSet.Count() == 0) return 1;
            else return dbSet.Max(x => x.Number) + 1;
        }*/

        public async Task AddImport(PurchaseInvoiceImport import)
        {
            await ImportsRepository.Add(import);
        }

        public async Task UpdateImport(PurchaseInvoiceImport import)
        {
            await ImportsRepository.Update(import);
        }

        public async Task<bool> RemoveImport(Guid id)
        {
            var import = ImportsRepository.Find(t => t.Id == id).FirstOrDefault();
            if (import != null)
            {
                await ImportsRepository.Remove(import);
                return true;
            }
            return false;
        }
    }
}
