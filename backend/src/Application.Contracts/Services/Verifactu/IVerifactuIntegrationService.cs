using Application.Contracts;
using Domain.Entities.Sales;

namespace Application.Contracts;

public interface IVerifactuIntegrationService
{
    Task<GenericResponse> FindInvoicesInVerifactu(int month, int year);
    Task<GenericResponse> SendInvoiceToVerifactu(Guid id);
    Task<GenericResponse> RemoveInvoiceFromVerifactu(Guid id);
    Task<IEnumerable<SalesInvoice>> GetInvoicesToIntegrateWithVerifactu(DateTime? toDate, Guid? initialStatusId);
    Task<IEnumerable<SalesInvoice>> GetIntegrationsBetweenDates(DateTime fromDate, DateTime toDate);
    Task<bool> HasInvoiceBeenIntegrated(Guid id);
    Task<IEnumerable<SalesInvoiceVerifactuRequest>> GetInvoiceRequests(Guid id);
    SalesInvoiceVerifactuRequest? GetLastSuccessfullRequest();
    Task<GenericResponse> CreateInvoiceRequest(SalesInvoiceVerifactuRequest verifactuRequest);
}
