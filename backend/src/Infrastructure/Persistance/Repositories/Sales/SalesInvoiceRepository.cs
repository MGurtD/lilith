using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories.Sales
{
    public class SalesInvoiceRepository(ApplicationDbContext context) : Repository<SalesInvoice, Guid>(context), ISalesInvoiceRepository
    {
        public IRepository<SalesInvoiceDetail, Guid> InvoiceDetails { get; } = new Repository<SalesInvoiceDetail, Guid>(context);
        public IRepository<SalesInvoiceImport, Guid> InvoiceImports { get; } = new Repository<SalesInvoiceImport, Guid>(context);
        public IRepository<SalesInvoiceDueDate, Guid> InvoiceDueDates { get; } = new Repository<SalesInvoiceDueDate, Guid>(context);

        public override async Task<SalesInvoice?> Get(Guid id)
        {
            return await dbSet
                        .Include(s => s.Customer)
                        .Include(s => s.Site)
                        .Include(s => s.SalesInvoiceDetails)
                            .ThenInclude(d => d.DeliveryNoteDetail)
                        .Include(s => s.SalesInvoiceImports)
                            .ThenInclude(d => d.Tax)
                        .Include(s => s.SalesInvoiceDueDates)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<SalesInvoice?> GetHeader(Guid id)
        {
            return await dbSet
                        .AsNoTracking()
                        .SingleOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<SalesInvoiceImport>> GetImportsByInvoiceId(Guid id)
        {
            var imports = new List<SalesInvoiceImport>();
            var invoice = await dbSet
                        .Include(s => s.SalesInvoiceImports)
                            .ThenInclude(d => d.Tax)
                        .FirstOrDefaultAsync(e => e.Id == id);

            if (invoice is not null) imports.AddRange(invoice.SalesInvoiceImports);
            return imports;
        }

        public async Task<IEnumerable<SalesInvoice>> GetPendingToIntegrate(DateTime? toDate, Guid? initialStatusId)
        {
            var query = dbSet
                        .AsNoTracking()
                        .Include(e => e.SalesInvoiceDueDates)
                        .Where(e => e.IntegrationStatusId != null);

            if (initialStatusId.HasValue)
            {
                query = query.Where(e => e.IntegrationStatusId == initialStatusId);
            }

            if (toDate.HasValue)
            {
                // Compare dates ignoring time by using an exclusive upper bound at the next day start
                var cutoff = toDate.Value.Date.AddDays(1);
                query = query.Where(e => e.InvoiceDate < cutoff);
            }

            return await query
                        .OrderBy(e => e.InvoiceNumber)
                        .ToListAsync();
        }

        public override IEnumerable<SalesInvoice> Find(Expression<Func<SalesInvoice, bool>> predicate)
        {
            return dbSet
                .AsNoTracking()
                .Include(d => d.SalesInvoiceDueDates)
                .Where(predicate)
                .OrderBy(pi => pi.InvoiceNumber);
        }

        public async Task<IEnumerable<SalesInvoice>> GetIntegrationsBetweenDates(DateTime fromDate, DateTime toDate)
        {
            // Return invoices that have at least one Verifactu request whose CreatedOn falls within the date range
            return await dbSet
                .AsNoTracking()
                .Include(i => i.VerifactuRequests)
                .Where(i => i.VerifactuRequests.Any(r => r.CreatedOn >= fromDate && r.CreatedOn <= toDate))
                .OrderBy(i => i.InvoiceNumber)
                .ToListAsync();
        }

    }
}