using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.Extensions.Options;
using Verifactu.Contracts;
using Verifactu;
using static Application.Contracts.StatusConstants;

namespace Application.Services.Verifactu;

public class VerifactuIntegrationService(IUnitOfWork unitOfWork,
    ISalesInvoiceService salesInvoiceService,
    IQrCodeService qrCodeService,
    ILocalizationService localizationService,
    IOptions<AppSettings> settings) : IVerifactuIntegrationService
{
    private readonly AppSettings settings = settings.Value;

    public async Task<GenericResponse> SendInvoiceToVerifactu(Guid id)
    {
        var validationResult = await ValidateInvoiceForVerifactu(id);
        if (!validationResult.Result) return validationResult;

        var invoice = await unitOfWork.SalesInvoices.Get(id);
        var enterprise = (await unitOfWork.Enterprises.FindAsync(e => !e.Disabled)).FirstOrDefault();
        var (lastSuccessfulRequest, lastSuccessfulInvoice) = await GetLastSuccessfulRequestAndInvoice();

        var request = new RegisterInvoiceRequest
        {
            Enterprise = enterprise!,
            SalesInvoice = invoice!,
            PreviousNotificatedInvoice = lastSuccessfulInvoice,
            PreviousHash = lastSuccessfulRequest?.Hash
        };

        var verifactuService = CreateVerifactuService();
        var response = await verifactuService.RegisterInvoice(request);

        return await ProcessVerifactuResponse(invoice!, response, true);
    }

    public async Task<GenericResponse> RemoveInvoiceFromVerifactu(Guid id)
    {
        var validationResult = await ValidateInvoiceExistsForVerifactu(id);
        if (!validationResult.Result) return validationResult;

        var invoice = await unitOfWork.SalesInvoices.Get(id);
        var enterprise = (await unitOfWork.Enterprises.FindAsync(e => !e.Disabled)).FirstOrDefault();
        var (lastSuccessfulRequest, lastSuccessfulInvoice) = await GetLastSuccessfulRequestAndInvoice();

        var request = new CancelInvoiceRequest
        {
            Enterprise = enterprise!,
            InvoiceToCancel = invoice!,
            PreviousNotificatedInvoice = lastSuccessfulInvoice,
            PreviousHash = lastSuccessfulRequest?.Hash
        };

        var verifactuService = CreateVerifactuService();
        var response = await verifactuService.CancelInvoice(request);

        return await ProcessVerifactuResponse(invoice!, response, false);
    }

    private async Task<GenericResponse> ValidateInvoiceForVerifactu(Guid id)
    {
        var invoice = await unitOfWork.SalesInvoices.Get(id);
        if (invoice == null) 
            return new GenericResponse(false, localizationService.GetLocalizedString("VerifactuInvoiceNotFound", id));
        
        if (invoice.SalesInvoiceDetails.Count == 0)
            return new GenericResponse(false, localizationService.GetLocalizedString("VerifactuInvoiceNoDetails"));

        return await ValidateEnterpriseExists();
    }

    private async Task<GenericResponse> ValidateInvoiceExistsForVerifactu(Guid id)
    {
        var invoice = await unitOfWork.SalesInvoices.Get(id);
        if (invoice == null) 
            return new GenericResponse(false, localizationService.GetLocalizedString("VerifactuInvoiceNotFound", id));

        return await ValidateEnterpriseExists();
    }

    private async Task<GenericResponse> ValidateEnterpriseExists()
    {
        var enterprise = (await unitOfWork.Enterprises.FindAsync(e => !e.Disabled)).FirstOrDefault();
        if (enterprise == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("VerifactuEnterpriseNotFound"));

        return new GenericResponse(true);
    }

    private async Task<(SalesInvoiceVerifactuRequest?, SalesInvoice?)> GetLastSuccessfulRequestAndInvoice()
    {
        var lastSuccessfulRequest = GetLastSuccessfullRequest();
        var lastSuccessfulInvoice = lastSuccessfulRequest != null
            ? await salesInvoiceService.GetHeaderById(lastSuccessfulRequest.SalesInvoiceId)
            : null;

        return (lastSuccessfulRequest, lastSuccessfulInvoice);
    }

    private VerifactuInvoiceService CreateVerifactuService()
    {
        var verifactuSettings = settings.Verifactu!;
        return new VerifactuInvoiceService(
            verifactuSettings.Url, 
            verifactuSettings.UrlQr, 
            verifactuSettings.Certificate.Path, 
            verifactuSettings.Certificate.Password);
    }

    private async Task<GenericResponse> ProcessVerifactuResponse(SalesInvoice invoice, dynamic response, bool updateInvoiceStatus)
    {
        var verifactuRequest = new SalesInvoiceVerifactuRequest
        {
            Hash = response.Hash,
            Request = response.XmlRequest,
            Response = response.XmlResponse,
            SalesInvoiceId = invoice.Id,
            Success = response.Success,
            Status = response.StatusRegister,
            TimestampResponse = response.Timestamp,
            QrCodeUrl = response.QrCodeUrl,
            QrCodeBase64 = qrCodeService.GeneratePngBase64(response.QrCodeUrl)
        };
        
        await CreateInvoiceRequest(verifactuRequest);

        // Update invoice integration status only for SendInvoiceToVerifactu
        if (updateInvoiceStatus)
        {
            invoice.IntegrationStatusId = response.Success
                ? (await unitOfWork.Lifecycles.GetStatusByName(Lifecycles.Verifactu, Statuses.Ok))?.Id
                : (await unitOfWork.Lifecycles.GetStatusByName(Lifecycles.Verifactu, Statuses.Error))?.Id;
            await unitOfWork.SalesInvoices.Update(invoice);
        }

        return new GenericResponse(true, verifactuRequest);
    }

    public async Task<IEnumerable<SalesInvoice>> GetInvoicesToIntegrateWithVerifactu(DateTime? toDate, Guid? initialStatusId)
    {
        return await unitOfWork.SalesInvoices.GetPendingToIntegrate(toDate, initialStatusId);
    }

    public async Task<IEnumerable<SalesInvoiceVerifactuRequest>> GetInvoiceRequests(Guid invoiceId)
    {
        return await unitOfWork.VerifactuRequests.FindAsync(i => i.SalesInvoiceId == invoiceId);
    }

    public async Task<IEnumerable<SalesInvoice>> GetIntegrationsBetweenDates(DateTime fromDate, DateTime toDate)
    {
        return await unitOfWork.SalesInvoices.GetIntegrationsBetweenDates(fromDate, toDate);
    }

    public async Task<bool> HasInvoiceBeenIntegrated(Guid id)
    {
        var requests = await GetInvoiceRequests(id);
        return requests.Any(r => r.Success);
    }

    public async Task<GenericResponse> CreateInvoiceRequest(SalesInvoiceVerifactuRequest verifactuRequest)
    {
        await unitOfWork.VerifactuRequests.Add(verifactuRequest);
        return new GenericResponse(true, verifactuRequest);
    }

    public SalesInvoiceVerifactuRequest? GetLastSuccessfullRequest()
    {
        var lastSuccessfullRequest = unitOfWork.VerifactuRequests.Find(vr => vr.Success == true)
            .OrderByDescending(vr => vr.CreatedOn)
            .FirstOrDefault();
        return lastSuccessfullRequest;
    }

    public async Task<GenericResponse> FindInvoicesInVerifactu(int month, int year)
    {
        var enterprise = (await unitOfWork.Enterprises.FindAsync(e => !e.Disabled)).FirstOrDefault();
        if (enterprise == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("VerifactuEnterpriseNotFound"));

        var site = (await unitOfWork.Sites.FindAsync(s => !s.Disabled)).FirstOrDefault();
        if (site == null)
            return new GenericResponse(false, localizationService.GetLocalizedString("VerifactuSiteNotFound"));

        // Construir la petició
        var request = new FindInvoicesRequest
        {
            EnterpriseName = enterprise.Name,
            VatNumber = site.VatNumber,
            Month = month,
            Year = year,
        };

        // Enviar la factura a Verifactu
        var settings = this.settings.Verifactu!;
        var verifactuInvoiceService = new VerifactuInvoiceService(settings.Url, settings.UrlQr, settings.Certificate.Path, settings.Certificate.Password);
        var response = await verifactuInvoiceService.FindInvoices(request);
        
        return new GenericResponse(true, response);
    }

}





