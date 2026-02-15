using Domain.Entities.Shared;

namespace Application.Contracts;

public interface IInvoiceSerieService
{
    Task<IEnumerable<InvoiceSerie>> GetAllInvoiceSeries();
    Task<InvoiceSerie?> GetInvoiceSerieById(Guid id);
    Task<GenericResponse> CreateInvoiceSerie(InvoiceSerie invoiceSerie);
    Task<GenericResponse> UpdateInvoiceSerie(InvoiceSerie invoiceSerie);
    Task<GenericResponse> RemoveInvoiceSerie(Guid id);
}
