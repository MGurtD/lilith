using System.Text.Json.Serialization;

namespace Application.Ingestion;

// Internal DTOs that mirror LlamaParse's structured extraction response shape.
// Not exposed via Application.Contracts — only the Application/Ingestion project sees these.
// Aligned with the current LlamaCloud SaaS API (developers.llamaindex.ai/llamaparse/extract/api):
//   - POST /api/v1/beta/files        → { id }
//   - POST /api/v2/extract?project_id → { id, status }
//   - GET  /api/v2/extract/{jobId}    → { id, status, extract_result, extract_metadata? }

internal class LlamaParseUploadResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

internal class LlamaParseExtractionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("extract_result")]
    public LlamaParseExtractionData? ExtractResult { get; set; }

    [JsonPropertyName("extract_metadata")]
    public LlamaParseExtractMetadata? ExtractMetadata { get; set; }
}

internal class LlamaParseExtractMetadata
{
    // Real LlamaCloud shape (verified in POC batch 7, 2026-06-30): confidence scores live under
    //   field_metadata.document_metadata.<field>.{parsing,extraction,confidence}
    // and the wrapper also carries parse_job_id / parse_tier.
    // The flat confidence_scores dict from the #498 study does NOT exist in the v2 response —
    // it deserializes to null (System.Text.Json ignores missing fields). Kept as an optional
    // property for forward-compat with future API versions that may expose it.
    [JsonPropertyName("field_metadata")]
    public LlamaParseFieldMetadata? FieldMetadata { get; set; }

    [JsonPropertyName("parse_job_id")]
    public string? ParseJobId { get; set; }

    [JsonPropertyName("parse_tier")]
    public string? ParseTier { get; set; }

    [JsonPropertyName("confidence_scores")]
    public Dictionary<string, decimal>? ConfidenceScores { get; set; }
}

internal class LlamaParseFieldMetadata
{
    [JsonPropertyName("document_metadata")]
    public System.Text.Json.JsonElement? DocumentMetadata { get; set; }
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