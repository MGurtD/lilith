namespace Application.Contracts.Ingestion;

public class TaxBreakdownRow
{
    public decimal TaxRate { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    /// <summary>Null when no single active tax matches the rate; the operator picks it.</summary>
    public Guid? TaxId { get; set; }
    /// <summary>Equivalence surcharge read from the document; informative, never imported.</summary>
    public decimal? SurchargeRate { get; set; }
    public decimal? SurchargeAmount { get; set; }
}
