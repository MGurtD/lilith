using Domain.Entities.Purchase;

namespace Application.Contracts;

public interface IReceiptService
{
    Task<Receipt?> GetById(Guid id);
    Task<IEnumerable<Receipt>> GetReceiptsByReferenceId(Guid referenceId);
    
    IEnumerable<Receipt> GetBetweenDates(DateTime startDate, DateTime endDate);
    IEnumerable<Receipt> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);
    IEnumerable<Receipt> GetBetweenDatesAndSupplier(DateTime startDate, DateTime endDate, Guid customerId);
    IEnumerable<Receipt> GetBySupplier(Guid supplierId, bool withoutInvoice);
    IEnumerable<Receipt> GetByInvoice(Guid invoiceId);
    IEnumerable<Receipt> GetByStatus(Guid statusId);

    Task<GenericResponse> Create(CreatePurchaseDocumentRequest createRequest);
    Task<GenericResponse> Update(Receipt receipt);
    Task<GenericResponse> Remove(Guid id);

    Task<GenericResponse> MoveToWarehose(Receipt receipt);
    Task<GenericResponse> RetriveFromWarehose(Receipt receipt);

    Task<GenericResponse> AddDetail(ReceiptDetail detail);
    Task<GenericResponse> UpdateDetail(ReceiptDetail detail);
    Task<GenericResponse> RemoveDetail(Guid id);
    Task<GenericResponse> CalculateDetailWeightAndPrice(ReceiptDetail detail);

    Task<List<PurchaseOrderReceiptDetail>> GetReceptions(Guid id);
    Task<GenericResponse> AddReceptions(AddReceptionsRequest request);
}
