using SistemaFacturacion;
using System.Security.Cryptography.X509Certificates;
using Verifactu.Contracts;
using Verifactu.Factories;
using Verifactu.Mappers;
using Verifactu.Utils;

namespace Verifactu;

public interface IVerifactuInvoiceService
{
    Task<FindInvoicesResponse> FindInvoices(FindInvoicesRequest inputRequest);
    Task<VerifactuResponse> RegisterInvoice(RegisterInvoiceRequest request);
    Task<VerifactuResponse> CancelInvoice(CancelInvoiceRequest request);
}

public class VerifactuInvoiceService : IVerifactuInvoiceService
{
    private readonly sfPortTypeVerifactuClient _client;
    private readonly string _wsUrl;
    private readonly string _qrUrl;

    public VerifactuInvoiceService(string wsUrl, string qrUrl, string certificatePath, string certificatePassword)
    {
        ValidateConstructorParameters(wsUrl, certificatePath, certificatePassword);
        _wsUrl = wsUrl;
        _qrUrl = qrUrl;
        _client = CreateAndConfigureClient(wsUrl, certificatePath, certificatePassword);
    }

    private static void ValidateConstructorParameters(string baseUrl, string certificatePath, string certificatePassword)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("La URL de Verifactu no está configurada.");
        }
        if (string.IsNullOrWhiteSpace(certificatePath))
        {
            throw new ArgumentException("La ruta del certificado no está configurada.");
        }
        if (string.IsNullOrWhiteSpace(certificatePassword))
        {
            throw new ArgumentException("La contraseña del certificado no está configurada.");
        }
    }

    private static sfPortTypeVerifactuClient CreateAndConfigureClient(string baseUrl, string certificatePath, string certificatePassword)
    {
        var client = new sfPortTypeVerifactuClient(
            baseUrl.Contains(VerifactuConstants.TestEndpointIdentifier)
                ? sfPortTypeVerifactuClient.EndpointConfiguration.SistemaVerifactuPruebas
                : sfPortTypeVerifactuClient.EndpointConfiguration.SistemaVerifactu);

        // Configurar certificado usando X509CertificateLoader
        client.ClientCredentials.ClientCertificate.Certificate = 
            X509CertificateLoader.LoadPkcs12FromFile(certificatePath, certificatePassword);

        // Configurar binding para seguridad de transporte
        var binding = (System.ServiceModel.BasicHttpBinding)client.Endpoint.Binding;
        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Certificate;

        return client;
    }

    public async Task<FindInvoicesResponse> FindInvoices(FindInvoicesRequest inputRequest)
    {
        var request = VerifactuRequestFactory.CreateFindInvoicesRequest(inputRequest);
        var response = await _client.ConsultaFactuSistemaFacturacionAsync(request);
        
        return VerifactuResponseMapper.MapFindInvoicesResponse(response);
    }

    public async Task<VerifactuResponse> RegisterInvoice(RegisterInvoiceRequest request)
    {
        var peticion = VerifactuRequestFactory.CreateRegisterInvoiceRequest(request);
        
        // Configurar encadenamiento y generar hash
        var registroFactura = peticion.RegistroFactura[0].Item;
        if (registroFactura is RegistroFacturacionAltaType alta)
        {
            ConfigureChaining(alta, request);
            var hash = HashGenerator.GenerateHashForInvoiceRegistration(alta, request.PreviousHash);
            alta.Huella = hash;

            var verifactuResponse = await _client.RegFactuSistemaFacturacionAsync(peticion);
            var response = VerifactuResponseMapper.MapRegisterInvoiceResponse(verifactuResponse, peticion, hash);

            // Generar la URL del QR
            response.QrCodeUrl = VerifactuFormatUtils.GenerateQrValidationUrl(
                _qrUrl,
                request.SalesInvoice.VatNumber,
                request.SalesInvoice.InvoiceNumber,
                request.SalesInvoice.InvoiceDate,
                request.SalesInvoice.BaseAmount + request.SalesInvoice.TaxAmount);

            return response;
        }
        
        throw new InvalidOperationException("Error al procesar el registro de factura");
    }

    private static void ConfigureChaining(RegistroFacturacionAltaType alta, RegisterInvoiceRequest request)
    {
        if (request.PreviousNotificatedInvoice == null)
        {
            alta.Encadenamiento.Item = PrimerRegistroCadenaType.S;
        }
        else
        {
            alta.Encadenamiento.Item = new EncadenamientoFacturaAnteriorType
            {
                NumSerieFactura = request.PreviousNotificatedInvoice.InvoiceNumber,
                FechaExpedicionFactura = VerifactuFormatUtils.FormatDate(request.PreviousNotificatedInvoice.InvoiceDate),
                IDEmisorFactura = request.PreviousNotificatedInvoice.VatNumber,
                Huella = request.PreviousHash
            };
        }
    }
    
    public async Task<VerifactuResponse> CancelInvoice(CancelInvoiceRequest request)
    {
        var peticion = VerifactuRequestFactory.CreateCancelInvoiceRequest(request);
        
        // Configurar encadenamiento y generar hash
        var registroFactura = peticion.RegistroFactura[0].Item;
        if (registroFactura is RegistroFacturacionAnulacionType anulacion)
        {
            ConfigureCancellationChaining(anulacion, request);
            var hash = HashGenerator.GenerateHashForInvoiceCancelation(anulacion, request.PreviousHash);
            anulacion.Huella = hash;
            
            var response = await _client.RegFactuSistemaFacturacionAsync(peticion);
            return VerifactuResponseMapper.MapCancelInvoiceResponse(response, peticion, hash);
        }
        
        throw new InvalidOperationException("Error al procesar la anulación de factura");
    }

    private static void ConfigureCancellationChaining(RegistroFacturacionAnulacionType anulacion, CancelInvoiceRequest request)
    {
        if (request.PreviousNotificatedInvoice == null)
        {
            anulacion.Encadenamiento.Item = PrimerRegistroCadenaType.S;
        }
        else
        {
            anulacion.Encadenamiento.Item = new EncadenamientoFacturaAnteriorType
            {
                NumSerieFactura = request.PreviousNotificatedInvoice.InvoiceNumber,
                FechaExpedicionFactura = VerifactuFormatUtils.FormatDate(request.PreviousNotificatedInvoice.InvoiceDate),
                IDEmisorFactura = request.PreviousNotificatedInvoice.VatNumber,
                Huella = request.PreviousHash
            };
        }
    }
}