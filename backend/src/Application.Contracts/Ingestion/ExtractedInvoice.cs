namespace Application.Contracts.Ingestion;

/// <summary>
/// Raw values read from a supplier invoice, before any catalog resolution or check.
/// </summary>
public class ExtractedInvoice
{
    public string? InvoiceNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public string? SupplierVatNumber { get; set; }
    public string? SupplierName { get; set; }
    public List<ExtractedTaxRow> TaxRows { get; set; } = [];
    public decimal? WithholdingPercentage { get; set; }
    public decimal? WithholdingAmount { get; set; }
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Provider confidence (0-1) per field, keyed by <see cref="ExtractedInvoiceFields"/>.
    /// Fields without a score are absent.
    /// </summary>
    public Dictionary<string, decimal> FieldConfidence { get; set; } = [];
}

public class ExtractedTaxRow
{
    public decimal? TaxRate { get; set; }
    public decimal? BaseAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? SurchargeRate { get; set; }
    public decimal? SurchargeAmount { get; set; }
}

public static class ExtractedInvoiceFields
{
    public const string InvoiceNumber = "invoiceNumber";
    public const string IssueDate = "issueDate";
    public const string SupplierVatNumber = "supplierVatNumber";
    public const string SupplierName = "supplierName";
    public const string WithholdingPercentage = "withholdingPercentage";
    public const string WithholdingAmount = "withholdingAmount";
    public const string TotalAmount = "totalAmount";

    public static string TaxRow(int index, string field) => $"taxRows[{index}].{field}";

    public const string TaxRate = "taxRate";
    public const string BaseAmount = "baseAmount";
    public const string TaxAmount = "taxAmount";
    public const string SurchargeRate = "surchargeRate";
    public const string SurchargeAmount = "surchargeAmount";
}
