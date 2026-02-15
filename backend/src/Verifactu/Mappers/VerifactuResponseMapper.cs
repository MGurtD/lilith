using SistemaFacturacion;
using Verifactu.Contracts;
using Verifactu.Utils;

namespace Verifactu.Mappers;

/// <summary>
/// Mapper para convertir respuestas del sistema Verifactu a objetos del dominio
/// </summary>
public static class VerifactuResponseMapper
{
    /// <summary>
    /// Mapea la respuesta de consulta de facturas a un objeto del dominio
    /// </summary>
    /// <param name="response">Respuesta del sistema Verifactu</param>
    /// <returns>Objeto del dominio con las facturas encontradas</returns>
    public static FindInvoicesResponse MapFindInvoicesResponse(ConsultaFactuSistemaFacturacionResponse response)
    {
        var respuesta = response.RespuestaConsultaFactuSistemaFacturacion;
        
        if (respuesta.ResultadoConsulta == ResultadoConsultaType.SinDatos)
        {
            return new FindInvoicesResponse();
        }

        var facturas = respuesta.RegistroRespuestaConsultaFactuSistemaFacturacion
            .Select(r => new Factura
            {
                IDEmisorFactura = r.IDFactura.IDEmisorFactura,
                NumSerieFactura = r.IDFactura.NumSerieFactura,
                FechaExpedicionFactura = r.IDFactura.FechaExpedicionFactura,
                ImporteTotal = r.DatosRegistroFacturacion.ImporteTotal,
                TipoFactura = r.DatosRegistroFacturacion.TipoFactura.ToString(),
                CuotaTotal = r.DatosRegistroFacturacion.CuotaTotal,
                FechaHoraUsoRegistro = r.DatosRegistroFacturacion.FechaHoraHusoGenRegistro,
                Huella = r.DatosRegistroFacturacion.Huella
            }).ToList();

        return new FindInvoicesResponse { Invoices = facturas };
    }

    /// <summary>
    /// Mapea la respuesta de registro de factura a un objeto del dominio
    /// </summary>
    /// <param name="response">Respuesta del sistema Verifactu</param>
    /// <param name="request">Solicitud original</param>
    /// <param name="hash">Hash generado</param>
    /// <returns>Objeto del dominio con el resultado del registro</returns>
    public static VerifactuResponse MapRegisterInvoiceResponse(
        RegFactuSistemaFacturacionResponse response,
        RegFactuSistemaFacturacion request,
        string hash)
    {
        var respuesta = response.RespuestaRegFactuSistemaFacturacion;
        var primeraLinea = respuesta.RespuestaLinea[0];
        
        var esExitoso = primeraLinea.EstadoRegistro == EstadoRegistroType.Correcto ||
                       primeraLinea.EstadoRegistro == EstadoRegistroType.AceptadoConErrores;

        var resultado = new VerifactuResponse
        {
            Hash = hash,
            Timestamp = DateTime.Now,
            XmlRequest = VerifactuFormatUtils.SerializeToXml(request),
            XmlResponse = VerifactuFormatUtils.SerializeToXml(respuesta),
            Success = esExitoso,
            StatusRegister = primeraLinea.EstadoRegistro.ToString()
        };

        if (!esExitoso)
        {
            resultado.ErrorMessage = $"{primeraLinea.CodigoErrorRegistro} - {primeraLinea.DescripcionErrorRegistro}";
        }

        return resultado;
    }

    /// <summary>
    /// Mapea la respuesta de anulación de factura a un objeto del dominio
    /// </summary>
    /// <param name="response">Respuesta del sistema Verifactu</param>
    /// <param name="request">Solicitud original</param>
    /// <param name="hash">Hash generado</param>
    /// <returns>Objeto del dominio con el resultado de la anulación</returns>
    public static VerifactuResponse MapCancelInvoiceResponse(
        RegFactuSistemaFacturacionResponse response,
        RegFactuSistemaFacturacion request,
        string hash)
    {
        var respuesta = response.RespuestaRegFactuSistemaFacturacion;
        var primeraLinea = respuesta.RespuestaLinea[0];
        
        var esExitoso = primeraLinea.EstadoRegistro == EstadoRegistroType.Correcto ||
                       primeraLinea.EstadoRegistro == EstadoRegistroType.AceptadoConErrores;

        var resultado = new VerifactuResponse
        {
            Hash = hash,
            Timestamp = DateTime.Now,
            XmlRequest = VerifactuFormatUtils.SerializeToXml(request),
            XmlResponse = VerifactuFormatUtils.SerializeToXml(respuesta),
            Success = esExitoso,
            StatusRegister = primeraLinea.EstadoRegistro.ToString()
        };

        if (!esExitoso)
        {
            resultado.ErrorMessage = $"{primeraLinea.CodigoErrorRegistro} - {primeraLinea.DescripcionErrorRegistro}";
        }

        return resultado;
    }
}
