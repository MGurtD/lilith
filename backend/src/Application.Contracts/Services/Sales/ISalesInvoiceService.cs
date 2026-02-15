using Application.Contracts;
using Domain.Entities.Sales;

namespace Application.Contracts;

public interface ISalesInvoiceService
{
    Task<GenericResponse> Create(CreateHeaderRequest createInvoiceRequest);
    Task<GenericResponse> CreateRectificative(CreateRectificativeInvoiceRequest dto);

    Task<SalesInvoice?> GetById(Guid id);
    Task<SalesInvoice?> GetHeaderById(Guid id);
    IEnumerable<SalesInvoice> GetBetweenDates(DateTime startDate, DateTime endDate);
    IEnumerable<SalesInvoice> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);
    IEnumerable<SalesInvoice> GetBetweenDatesAndExcludeStatus(DateTime startDate, DateTime endDate, Guid excludeStatusId);
    IEnumerable<SalesInvoice> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId);
    IEnumerable<SalesInvoice> GetByCustomer(Guid customerId);
    IEnumerable<SalesInvoice> GetByStatus(Guid statusId);
    IEnumerable<SalesInvoice> GetByExercise(Guid exerciseId);

    Task<GenericResponse> Update(SalesInvoice SalesInvoice);
    Task<GenericResponse> ChangeStatuses(ChangeStatusOfInvoicesRequest changeStatusOfPurchaseInvoicesRequest);
    Task<GenericResponse> Remove(Guid id);

    Task<GenericResponse> AddDeliveryNote(Guid id, DeliveryNote deliveryNote);
    Task<GenericResponse> RemoveDeliveryNote(Guid id, DeliveryNote deliveryNote);

    Task<GenericResponse> AddDetail(SalesInvoiceDetail detail);
    Task<GenericResponse> UpdateDetail(SalesInvoiceDetail detail);
    Task<GenericResponse> RemoveDetail(Guid id);

}
