namespace Application.Contracts.Ingestion;

/// <summary>
/// An uninvoiced receipt (delivery note) of the draft's supplier, with its amount and
/// whether the ingestion suggests linking it to the invoice.
/// </summary>
public class ReceiptCandidate
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    /// <summary>The supplier's own delivery-note number.</summary>
    public string SupplierNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    /// <summary>Sum of the receipt lines, without VAT.</summary>
    public decimal Amount { get; set; }
    public bool Suggested { get; set; }
    /// <summary>Why it is suggested; see <see cref="ReceiptMatchReasons"/>.</summary>
    public string? MatchReason { get; set; }
}

public static class ReceiptMatchReasons
{
    /// <summary>Its delivery-note number is printed on the invoice.</summary>
    public const string DeliveryNoteNumber = "DeliveryNoteNumber";
    /// <summary>The suggested receipts add up to the invoice taxable base.</summary>
    public const string Amount = "Amount";
}
