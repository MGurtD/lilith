namespace Application.Contracts.Ingestion;

public class TaxBreakdownRow
{
    public decimal TaxRate { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public Guid TaxId { get; set; }
    public decimal Confidence { get; set; }
}