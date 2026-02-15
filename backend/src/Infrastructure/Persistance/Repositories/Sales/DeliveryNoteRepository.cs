using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class DeliveryNoteRepository : Repository<DeliveryNote, Guid>, IDeliveryNoteRepository
    {
        public IRepository<DeliveryNoteDetail, Guid> Details { get; }

        public DeliveryNoteRepository(ApplicationDbContext context) : base(context) 
        {
            Details = new Repository<DeliveryNoteDetail, Guid>(context);
        }

        public override async Task<DeliveryNote?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Details)
                            .ThenInclude(d => d.Reference)
                        .Include(d => d.SalesInvoice)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public IEnumerable<DeliveryNote> GetByInvoiceId(Guid invoiceId)
        {
            return dbSet
                    .Include(d => d.Details)
                    .AsNoTracking()
                    .Where(e => e.SalesInvoiceId == invoiceId);
        }

    }
}
