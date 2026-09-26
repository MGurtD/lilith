using System.Text.Json;
using Application.Contracts.Ingestion;
using Infrastructure.Ingestion;
using Xunit;

namespace Application.Tests.Ingestion;

/// <summary>
/// Unit tests for <see cref="LlamaCloudExtractionMapper"/> — issue #78.
/// Uses canned LlamaCloud extract job payloads.
/// </summary>
public class LlamaCloudExtractionMapperTests
{
    [Fact]
    public void Maps_header_tax_rows_withholding_and_total()
    {
        var result = Json("""
        {
          "invoice_number": "F-2026/0042",
          "issue_date": "2026-06-15",
          "supplier": { "vat_number": "B12345674", "name": "Acme SL" },
          "tax_breakdown": [
            { "tax_rate": 21, "base_amount": 700.0, "tax_amount": 147.0, "surcharge_rate": null, "surcharge_amount": null },
            { "tax_rate": 10, "base_amount": 300.0, "tax_amount": 30.0 }
          ],
          "withholding": { "percentage": 15, "amount": -150.0 },
          "total_amount": 1027.0
        }
        """);

        var invoice = LlamaCloudExtractionMapper.Map(result, null);

        Assert.Equal("F-2026/0042", invoice.InvoiceNumber);
        Assert.Equal(new DateTime(2026, 6, 15), invoice.IssueDate);
        Assert.Equal("B12345674", invoice.SupplierVatNumber);
        Assert.Equal("Acme SL", invoice.SupplierName);
        Assert.Equal(2, invoice.TaxRows.Count);
        Assert.Equal(147m, invoice.TaxRows[0].TaxAmount);
        Assert.Null(invoice.TaxRows[0].SurchargeRate);
        Assert.Equal(15m, invoice.WithholdingPercentage);
        Assert.Equal(150m, invoice.WithholdingAmount);
        Assert.Equal(1027m, invoice.TotalAmount);
    }

    [Fact]
    public void Reads_numbers_and_dates_returned_as_localized_strings()
    {
        var result = Json("""
        {
          "issue_date": "15/06/2026",
          "tax_breakdown": [ { "tax_rate": "21 %", "base_amount": "1.234,56", "tax_amount": "259,26" } ],
          "total_amount": "1,493.82"
        }
        """);

        var invoice = LlamaCloudExtractionMapper.Map(result, null);

        Assert.Equal(new DateTime(2026, 6, 15), invoice.IssueDate);
        Assert.Equal(21m, invoice.TaxRows[0].TaxRate);
        Assert.Equal(1234.56m, invoice.TaxRows[0].BaseAmount);
        Assert.Equal(259.26m, invoice.TaxRows[0].TaxAmount);
        Assert.Equal(1493.82m, invoice.TotalAmount);
    }

    [Fact]
    public void Missing_or_malformed_values_become_null()
    {
        var result = Json("""{ "issue_date": "soon", "total_amount": "n/a", "supplier": "Acme" }""");

        var invoice = LlamaCloudExtractionMapper.Map(result, null);

        Assert.Null(invoice.IssueDate);
        Assert.Null(invoice.TotalAmount);
        Assert.Null(invoice.SupplierVatNumber);
        Assert.Empty(invoice.TaxRows);
    }

    [Fact]
    public void Collects_leaf_confidence_mirroring_the_schema()
    {
        var metadata = Json("""
        {
          "field_metadata": {
            "invoice_number": { "confidence": 0.97, "extraction_confidence": 0.98, "parsing_confidence": 0.96 },
            "supplier": { "vat_number": { "confidence": 0.62 } },
            "tax_breakdown": [
              { "tax_rate": { "confidence": 0.99 }, "tax_amount": { "confidence": 0.41 } }
            ],
            "unknown_field": { "confidence": 0.1 }
          }
        }
        """);

        var invoice = LlamaCloudExtractionMapper.Map(Json("{}"), metadata);

        Assert.Equal(0.97m, invoice.FieldConfidence[ExtractedInvoiceFields.InvoiceNumber]);
        Assert.Equal(0.62m, invoice.FieldConfidence[ExtractedInvoiceFields.SupplierVatNumber]);
        Assert.Equal(0.41m, invoice.FieldConfidence[ExtractedInvoiceFields.TaxRow(0, ExtractedInvoiceFields.TaxAmount)]);
        // invoice_number, supplier.vat_number and both tax row leaves; unknown_field is dropped.
        Assert.Equal(4, invoice.FieldConfidence.Count);
    }

    [Fact]
    public void Accepts_confidence_nested_under_document_metadata()
    {
        var metadata = Json("""
        { "field_metadata": { "document_metadata": { "total_amount": { "confidence": 0.55 } } } }
        """);

        var invoice = LlamaCloudExtractionMapper.Map(Json("{}"), metadata);

        Assert.Equal(0.55m, invoice.FieldConfidence[ExtractedInvoiceFields.TotalAmount]);
    }

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement;
}
