using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Purchase;

namespace Application.Contracts
{
    public interface IPurchaseInvoiceService
    {
        Task<GenericResponse> Create(PurchaseInvoice purchaseInvoice);

        Task<PurchaseInvoice?> GetById(Guid id);
        IEnumerable<PurchaseInvoice> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<PurchaseInvoice> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);
        IEnumerable<PurchaseInvoice> GetBetweenDatesAndExcludeStatus(DateTime startDate, DateTime endDate, Guid statusId);
        IEnumerable<PurchaseInvoice> GetBetweenDatesAndSupplier(DateTime startDate, DateTime endDate, Guid supplierId);
        IEnumerable<PurchaseInvoice> GetBetweenDatesExcludingStatusAndSupplier(DateTime startDate, DateTime endDate, Guid excludeStatusId, Guid supplierId);
        Task<IEnumerable<PurchaseInvoice>> GetByExercise(Guid exerciseId);

        Task<GenericResponse> RecreateDueDates(PurchaseInvoice purchaseInvoice);
        Task<GenericResponse> ChangeStatus(ChangeStatusRequest changeStatusOfPurchaseInvoiceRequest);
        Task<GenericResponse> ChangeStatuses(ChangeStatusOfInvoicesRequest changeStatusOfPurchaseInvoicesRequest);
        Task<GenericResponse> Update(PurchaseInvoice purchaseInvoice);
        Task<GenericResponse> Remove(Guid id);

        Task<GenericResponse> AddImport(PurchaseInvoiceImport import);
        Task<GenericResponse> UpdateImport(PurchaseInvoiceImport import);
        Task<GenericResponse> RemoveImport(Guid id);

        Task<GenericResponse> AddDueDates(IEnumerable<PurchaseInvoiceDueDate> dueDates);
        Task<GenericResponse> RemoveDueDates(IEnumerable<Guid> ids);
    }
}
