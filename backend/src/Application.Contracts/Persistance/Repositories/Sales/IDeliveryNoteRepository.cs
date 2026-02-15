using Domain.Entities.Sales;

namespace Application.Contracts
{
    public interface IDeliveryNoteRepository : IRepository<DeliveryNote, Guid>
    {
        IRepository<DeliveryNoteDetail, Guid> Details { get; }

        IEnumerable<DeliveryNote> GetByInvoiceId(Guid invoiceId);
    }
}
