namespace Application.Contracts.Ingestion;

/// <summary>
/// Draft purchase invoice values read from a supplier invoice PDF, plus the issues
/// the operator must review before saving.
/// </summary>
public class IngestPurchaseInvoiceResponse
{
    public string? SupplierVatNumber { get; set; }
    public string? SupplierName { get; set; }
    /// <summary>Set only when exactly one active supplier matches the VAT number.</summary>
    public Guid? SupplierId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    /// <summary>Withholding (IRPF) percentage, mapped to PurchaseInvoice.ExtraTaxPercentatge.</summary>
    public decimal? ExtraTaxPercentatge { get; set; }
    /// <summary>Invoice total as printed on the document.</summary>
    public decimal? TotalAmount { get; set; }
    public List<TaxBreakdownRow> TaxBreakdown { get; set; } = [];
    public List<IngestionIssue> Issues { get; set; } = [];
}
