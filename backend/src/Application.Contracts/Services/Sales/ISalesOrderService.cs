using Application.Contracts;
using Domain.Entities.Sales;
using SalesOrderDetail = Domain.Entities.Sales.SalesOrderDetail;

namespace Application.Contracts
{
    public interface ISalesOrderService
    {
        Task<GenericResponse> Create(CreateHeaderRequest createRequest);
        Task<GenericResponse> CreateFromBudget(Budget budget);

        SalesOrderHeader? GetOrderFromBudget(Guid id);
        Task<SalesOrderHeader?> GetById(Guid id);
        IEnumerable<SalesOrderHeader> GetByDeliveryNoteId(Guid deliveryNoteId);
        IEnumerable<SalesOrderHeader> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<SalesOrderHeader> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId);
        IEnumerable<SalesOrderHeader> GetOrdersToDeliver(Guid customerId);

        Task<GenericResponse> Deliver(Guid deliveryNoteId);
        Task<GenericResponse> UnDeliver(Guid deliveryNoteId);
        Task<GenericResponse> Update(SalesOrderHeader salesOrderHeader);
        Task<GenericResponse> Remove(Guid id);

        Task<GenericResponse> UpdateCosts(Guid id);

        Task<SalesOrderDetail?> GetDetailById(Guid id);
        Task<GenericResponse> AddDetail(SalesOrderDetail detail);
        Task<GenericResponse> UpdateDetail(SalesOrderDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);
    }
}
