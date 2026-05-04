using Application.Contracts;
using Domain.Entities.Sales;

namespace Application.Contracts
{
    public interface IDeliveryNoteService
    {
        Task<DeliveryNote?> GetById(Guid id);
        IEnumerable<DeliveryNote> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<DeliveryNote> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);
        IEnumerable<DeliveryNote> GetByStatus(Guid statusId);
        IEnumerable<DeliveryNote> GetByCustomer(Guid customerId);
        IEnumerable<DeliveryNote> GetByStatusAndCustomer(Guid statusId, Guid customerId);
        IEnumerable<DeliveryNote> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId);
        IEnumerable<DeliveryNote> GetBySalesInvoice(Guid salesInvoiceId);
        IEnumerable<DeliveryNote> GetDeliveryNotesToInvoice(Guid customerId);

        Task<GenericResponse> Create(CreateHeaderRequest createRequest);
        Task<GenericResponse> Update(DeliveryNote deliveryNote);
        Task<GenericResponse> Remove(Guid id);

        Task<GenericResponse> Deliver(DeliveryNote deliveryNote);
        Task<GenericResponse> UnDeliver(DeliveryNote deliveryNote);

        Task<GenericResponse> Invoice(Guid salesOrderId);
        Task<GenericResponse> UnInvoice(Guid salesOrderId);

        Task<GenericResponse> AddOrder(Guid deliveryNoteId, SalesOrderHeader order);
        Task<GenericResponse> RemoveOrder(Guid deliveryNoteId, SalesOrderHeader order);
    }
}
