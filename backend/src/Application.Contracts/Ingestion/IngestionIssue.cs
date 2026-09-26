namespace Application.Contracts.Ingestion;

/// <summary>
/// A value the operator must review. <see cref="Field"/> uses the purchase invoice
/// form field names; tax breakdown issues also carry the row index.
/// </summary>
public class IngestionIssue
{
    public string Field { get; set; } = string.Empty;
    public int? RowIndex { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public static class IngestionIssueFields
{
    public const string SupplierId = "supplierId";
    public const string SupplierNumber = "supplierNumber";
    public const string PurchaseInvoiceDate = "purchaseInvoiceDate";
    public const string ExtraTaxPercentatge = "extraTaxPercentatge";
    public const string NetAmount = "netAmount";
    public const string TaxBreakdown = "taxBreakdown";
}

public static class IngestionIssueCodes
{
    public const string MissingValue = "MissingValue";
    public const string LowConfidence = "LowConfidence";
    public const string InvalidVatNumber = "InvalidVatNumber";
    public const string SupplierNotFound = "SupplierNotFound";
    public const string SupplierAmbiguous = "SupplierAmbiguous";
    public const string UnknownTaxRate = "UnknownTaxRate";
    public const string AmbiguousTaxRate = "AmbiguousTaxRate";
    public const string TaxAmountMismatch = "TaxAmountMismatch";
    public const string SurchargeNotImported = "SurchargeNotImported";
    public const string TotalMismatch = "TotalMismatch";
}
