using SistemaFacturacion;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Verifactu;

public static class HashGenerator
{

    /// <summary>
    /// Construye la referencia de registro para alta concatenando pares "nombre=valor" separados por '&'.
    /// </summary>
    public static string GenerateHashForInvoiceRegistration(RegistroFacturacionAltaType alta, string? huellaAnterior)
    {
        //IDEmisorFactura=B09680521&amp;NumSerieFactura=25077&amp;FechaExpedicionFactura=03-07-2025&amp;TipoFactura=F1&amp;CuotaTotal=33.60&amp;ImporteTotal=193.60&amp;Huella=27A8309B67AED06176F80F204BB25F59874806A4CCE9EB475F31F040505A2745&amp;FechaHoraHusoGenRegistro=2025-07-03T18:28:08+02:00
        var sb = new StringBuilder();
        sb
            .Append(GetValorCampo("IDEmisorFactura", alta.IDFactura.IDEmisorFactura, true))
            .Append(GetValorCampo("NumSerieFactura", alta.IDFactura.NumSerieFactura, true))
            .Append(GetValorCampo("FechaExpedicionFactura", alta.IDFactura.FechaExpedicionFactura, true))
            .Append(GetValorCampo("TipoFactura", alta.TipoFactura.ToString(), true))
            .Append(GetValorCampo("CuotaTotal", alta.CuotaTotal, true))
            .Append(GetValorCampo("ImporteTotal", alta.ImporteTotal, true))
            .Append(GetValorCampo("Huella", huellaAnterior, true))
            .Append(GetValorCampo("FechaHoraHusoGenRegistro", alta.FechaHoraHusoGenRegistro, false));

        var stringToHash = sb.ToString();
        return GetHashVerifactu(stringToHash);
    }

    /// <summary>
    /// Construye la referencia de registro para alta concatenando pares "nombre=valor" separados por '&'.
    /// </summary>
    public static string GenerateHashForInvoiceCancelation(RegistroFacturacionAnulacionType anulacion, string? huellaAnterior)
    {
        var sb = new StringBuilder();
        sb
            .Append(GetValorCampo("IDEmisorFacturaAnulada", anulacion.IDFactura.IDEmisorFacturaAnulada, true))
            .Append(GetValorCampo("NumSerieFacturaAnulada", anulacion.IDFactura.NumSerieFacturaAnulada, true))
            .Append(GetValorCampo("FechaExpedicionFacturaAnulada", anulacion.IDFactura.FechaExpedicionFacturaAnulada, true))
            .Append(GetValorCampo("Huella", huellaAnterior, true))
            .Append(GetValorCampo("FechaHoraHusoGenRegistro", anulacion.FechaHoraHusoGenRegistro, false));

        return GetHashVerifactu(sb.ToString());
    }

    /// <summary>
    /// Genera el hash SHA-256 de la cadena de texto dada y lo devuelve en
    /// formato hexadecimal en MAYÚSCULAS (64 caracteres: 32 bytes * 2 dígitos).
    /// </summary>
    public static string GetHashVerifactu(string msg)
    {
        ArgumentNullException.ThrowIfNull(msg);

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            byte[] hashBytes = sha256.ComputeHash(bytes);

            // Convertir el hash a una cadena hexadecimal
            StringBuilder hash = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString().ToUpper();
        }
    }


    /// <summary>
    /// Devuelve "nombre=valor" y agrega '&' al final si separador == true.
    /// </summary>
    public static string GetValorCampo(string nombre, string? valor, bool separador)
    {
        var raw = string.IsNullOrEmpty(valor) ? string.Empty : valor.Trim();
        string campo = $"{nombre}={raw}";
        return separador ? campo + "&" : campo;
    }

    /// <summary>
    /// Devuelve "nombre=valorEncoded" y agrega '&' al final si separador == true. Utiliza UTF-8 para el URL encoding.
    /// </summary>
    public static string GetValorCampoEncoded(string nombre, string valor, bool separador)
    {
        // En .NET Core/Standard, WebUtility.UrlEncode aplica UTF-8 por defecto.
        string encoded = valor == null ? string.Empty : WebUtility.UrlEncode(valor);
        string campo = $"{nombre}={encoded}";
        return separador ? campo + "&" : campo;
    }

}
