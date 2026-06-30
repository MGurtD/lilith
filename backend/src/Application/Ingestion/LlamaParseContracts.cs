using System.Text.Json.Serialization;

namespace Application.Ingestion;

// Internal DTOs that mirror LlamaParse's structured extraction response shape.
// Not exposed via Application.Contracts — only the Application/Ingestion project sees these.
// POC-1 documented the chosen path: /api/parsing/upload + /api/extraction/run with an
// invoice JSON schema. Real provider shapes will be validated in a follow-up POC.

internal class LlamaParseUploadResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

internal class LlamaParseExtractionResponse
{
    [JsonPropertyName("data")]
    public LlamaParseExtractionData? Data { get; set; }
}

internal class LlamaParseExtractionData
{
    [JsonPropertyName("invoice_number")]
    public string? InvoiceNumber { get; set; }

    [JsonPropertyName("issue_date")]
    public DateTime? IssueDate { get; set; }

    [JsonPropertyName("supplier")]
    public LlamaParseSupplier? Supplier { get; set; }

    [JsonPropertyName("totals")]
    public LlamaParseTotals? Totals { get; set; }

    [JsonPropertyName("tax_breakdown")]
    public List<LlamaParseTaxRow> TaxBreakdown { get; set; } = new();

    [JsonPropertyName("confidence")]
    public LlamaParseConfidence? Confidence { get; set; }
}

internal class LlamaParseSupplier
{
    [JsonPropertyName("vat_number")]
    public string? VatNumber { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal class LlamaParseTotals
{
    [JsonPropertyName("base_amount")]
    public decimal? BaseAmount { get; set; }

    [JsonPropertyName("transport_amount")]
    public decimal? TransportAmount { get; set; }

    [JsonPropertyName("discount_percentage")]
    public decimal? DiscountPercentage { get; set; }

    [JsonPropertyName("extra_tax_percentage")]
    public decimal? ExtraTaxPercentage { get; set; }
}

internal class LlamaParseTaxRow
{
    [JsonPropertyName("tax_rate")]
    public decimal TaxRate { get; set; }

    [JsonPropertyName("base_amount")]
    public decimal BaseAmount { get; set; }

    [JsonPropertyName("tax_amount")]
    public decimal TaxAmount { get; set; }

    [JsonPropertyName("surcharge_rate")]
    public decimal? SurchargeRate { get; set; }

    [JsonPropertyName("surcharge_amount")]
    public decimal? SurchargeAmount { get; set; }

    [JsonPropertyName("confidence")]
    public decimal? Confidence { get; set; }
}

internal class LlamaParseConfidence
{
    [JsonPropertyName("headers")]
    public Dictionary<string, decimal> Headers { get; set; } = new();

    [JsonPropertyName("lines")]
    public List<decimal> Lines { get; set; } = new();
}