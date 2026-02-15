
namespace Verifactu.Contracts;

public class FindInvoicesResponse
{
    public List<Factura> Invoices { get; set; } = [];
}

public class Factura
{
    public string NumSerieFactura { get; set; } = string.Empty;
    public string FechaExpedicionFactura { get; set; } = string.Empty;
    public string ImporteTotal { get; internal set; } = string.Empty;
    public string IDEmisorFactura { get; internal set; } = string.Empty;
    public string Huella { get; internal set; } = string.Empty;
    public string TipoFactura { get; internal set; } = string.Empty;
    public string CuotaTotal { get; internal set; } = string.Empty;
    public DateTime FechaHoraUsoRegistro { get; internal set; }
}