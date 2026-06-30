namespace Application.Contracts.Ingestion;

public class IngestPurchaseInvoiceResponse
{
    public string? SupplierVatNumber { get; set; }
    public string? SupplierName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public decimal? BaseAmount { get; set; }
    public decimal? TransportAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? ExtraTaxPercentatge { get; set; }
    public List<TaxBreakdownRow> TaxBreakdown { get; set; } = new();
    public ConfidenceMap Confidence { get; set; } = new();
}