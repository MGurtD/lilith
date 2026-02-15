namespace Verifactu.Contracts;

public class FindInvoicesRequest
{
    public string VatNumber { get; set; } = string.Empty;
    public string EnterpriseName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
}