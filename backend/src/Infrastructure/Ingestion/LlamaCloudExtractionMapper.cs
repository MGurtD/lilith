using System.Globalization;
using System.Text.Json;
using Application.Contracts.Ingestion;

namespace Infrastructure.Ingestion;

/// <summary>
/// Maps a LlamaCloud extract job result to the provider-neutral <see cref="ExtractedInvoice"/>.
/// Reading is lenient: models sometimes return numbers and dates as localized strings.
/// </summary>
public static class LlamaCloudExtractionMapper
{
    private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "dd.MM.yyyy"];

    /// <summary>JSON schema sent to LlamaCloud; property names are read back by <see cref="Map"/>.</summary>
    public static object DataSchema { get; } = new Dictionary<string, object>
    {
        ["type"] = "object",
        ["properties"] = new Dictionary<string, object>
        {
            ["invoice_number"] = new { type = "string", description = "Supplier invoice number exactly as printed" },
            ["issue_date"] = new { type = "string", description = "Invoice issue date (fecha de expedición), formatted YYYY-MM-DD" },
            ["supplier"] = new
            {
                type = "object",
                description = "The company that issues the invoice (emisor), never the customer",
                properties = new Dictionary<string, object>
                {
                    ["vat_number"] = new { type = "string", description = "Issuer tax ID (NIF/CIF/VAT number), without spaces" },
                    ["name"] = new { type = "string", description = "Issuer legal name" },
                },
            },
            ["tax_breakdown"] = new
            {
                type = "array",
                description = "One entry per VAT (IVA) rate in the invoice tax summary",
                items = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["tax_rate"] = new { type = "number", description = "VAT (IVA) percentage, e.g. 21" },
                        ["base_amount"] = new { type = "number", description = "Taxable base (base imponible) for this rate" },
                        ["tax_amount"] = new { type = "number", description = "VAT amount (cuota) for this rate" },
                        ["surcharge_rate"] = new { type = "number", description = "Equivalence surcharge (recargo de equivalencia) percentage for this rate, if any" },
                        ["surcharge_amount"] = new { type = "number", description = "Equivalence surcharge amount for this rate, if any" },
                    },
                },
            },
            ["withholding"] = new
            {
                type = "object",
                description = "Income tax withholding (retención IRPF) deducted from the total, if any",
                properties = new Dictionary<string, object>
                {
                    ["percentage"] = new { type = "number", description = "Withholding percentage, e.g. 15" },
                    ["amount"] = new { type = "number", description = "Withholding amount, as a positive number" },
                },
            },
            ["total_amount"] = new { type = "number", description = "Invoice total to pay (total factura)" },
        },
    };

    public static ExtractedInvoice Map(JsonElement extractResult, JsonElement? extractMetadata)
    {
        var invoice = new ExtractedInvoice();
        if (extractResult.ValueKind != JsonValueKind.Object) return invoice;

        invoice.InvoiceNumber = ReadString(extractResult, "invoice_number");
        invoice.IssueDate = ReadDate(extractResult, "issue_date");

        if (TryGetObject(extractResult, "supplier", out var supplier))
        {
            invoice.SupplierVatNumber = ReadString(supplier, "vat_number");
            invoice.SupplierName = ReadString(supplier, "name");
        }

        if (extractResult.TryGetProperty("tax_breakdown", out var rows) && rows.ValueKind == JsonValueKind.Array)
        {
            foreach (var row in rows.EnumerateArray().Where(r => r.ValueKind == JsonValueKind.Object))
            {
                invoice.TaxRows.Add(new ExtractedTaxRow
                {
                    TaxRate = ReadDecimal(row, "tax_rate"),
                    BaseAmount = ReadDecimal(row, "base_amount"),
                    TaxAmount = ReadDecimal(row, "tax_amount"),
                    SurchargeRate = ReadDecimal(row, "surcharge_rate"),
                    SurchargeAmount = ReadDecimal(row, "surcharge_amount"),
                });
            }
        }

        if (TryGetObject(extractResult, "withholding", out var withholding))
        {
            invoice.WithholdingPercentage = Abs(ReadDecimal(withholding, "percentage"));
            invoice.WithholdingAmount = Abs(ReadDecimal(withholding, "amount"));
        }

        invoice.TotalAmount = ReadDecimal(extractResult, "total_amount");
        invoice.FieldConfidence = MapConfidence(extractMetadata);
        return invoice;
    }

    private static Dictionary<string, decimal> MapConfidence(JsonElement? extractMetadata)
    {
        var result = new Dictionary<string, decimal>();
        if (extractMetadata is not { ValueKind: JsonValueKind.Object } metadata
            || !metadata.TryGetProperty("field_metadata", out var fieldMetadata))
        {
            return result;
        }

        var scores = new Dictionary<string, decimal>();
        CollectScores(fieldMetadata, string.Empty, scores);

        foreach (var (path, score) in scores)
        {
            var field = ToNeutralField(path);
            if (field is not null) result[field] = score;
        }
        return result;
    }

    /// <summary>
    /// Walks the metadata tree, which mirrors the schema, and records every leaf that
    /// carries a numeric "confidence" under its dotted path (array items use their index).
    /// </summary>
    private static void CollectScores(JsonElement node, string path, Dictionary<string, decimal> scores)
    {
        switch (node.ValueKind)
        {
            case JsonValueKind.Object:
                if (node.TryGetProperty("confidence", out var confidence) && confidence.ValueKind == JsonValueKind.Number)
                {
                    scores[path] = confidence.GetDecimal();
                    return;
                }
                foreach (var property in node.EnumerateObject())
                    CollectScores(property.Value, Join(path, property.Name), scores);
                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in node.EnumerateArray())
                    CollectScores(item, Join(path, (index++).ToString(CultureInfo.InvariantCulture)), scores);
                break;
        }
    }

    private static string? ToNeutralField(string path)
    {
        // Some API versions nest the tree under "document_metadata".
        const string documentPrefix = "document_metadata.";
        if (path.StartsWith(documentPrefix, StringComparison.Ordinal))
            path = path[documentPrefix.Length..];

        var parts = path.Split('.');
        return parts switch
        {
            ["invoice_number"] => ExtractedInvoiceFields.InvoiceNumber,
            ["issue_date"] => ExtractedInvoiceFields.IssueDate,
            ["supplier", "vat_number"] => ExtractedInvoiceFields.SupplierVatNumber,
            ["supplier", "name"] => ExtractedInvoiceFields.SupplierName,
            ["withholding", "percentage"] => ExtractedInvoiceFields.WithholdingPercentage,
            ["withholding", "amount"] => ExtractedInvoiceFields.WithholdingAmount,
            ["total_amount"] => ExtractedInvoiceFields.TotalAmount,
            ["tax_breakdown", var index, var field] when int.TryParse(index, out var i) => field switch
            {
                "tax_rate" => ExtractedInvoiceFields.TaxRow(i, ExtractedInvoiceFields.TaxRate),
                "base_amount" => ExtractedInvoiceFields.TaxRow(i, ExtractedInvoiceFields.BaseAmount),
                "tax_amount" => ExtractedInvoiceFields.TaxRow(i, ExtractedInvoiceFields.TaxAmount),
                "surcharge_rate" => ExtractedInvoiceFields.TaxRow(i, ExtractedInvoiceFields.SurchargeRate),
                "surcharge_amount" => ExtractedInvoiceFields.TaxRow(i, ExtractedInvoiceFields.SurchargeAmount),
                _ => null,
            },
            _ => null,
        };
    }

    private static string Join(string path, string segment) =>
        path.Length == 0 ? segment : $"{path}.{segment}";

    private static bool TryGetObject(JsonElement parent, string name, out JsonElement value) =>
        parent.TryGetProperty(name, out value) && value.ValueKind == JsonValueKind.Object;

    private static string? ReadString(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value)) return null;
        var text = value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null,
        };
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    private static decimal? ReadDecimal(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind == JsonValueKind.Number) return value.GetDecimal();
        return value.ValueKind == JsonValueKind.String ? ParseDecimal(value.GetString()) : null;
    }

    /// <summary>
    /// Parses "1234.56", "1,234.56", "1.234,56" and "1234,56"; the last separator is the decimal one.
    /// </summary>
    internal static decimal? ParseDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var cleaned = new string(text.Where(c => char.IsDigit(c) || c is ',' or '.' or '-').ToArray());
        if (cleaned.Length == 0) return null;

        var lastComma = cleaned.LastIndexOf(',');
        var lastDot = cleaned.LastIndexOf('.');
        if (lastComma > lastDot)
            cleaned = cleaned.Replace(".", string.Empty).Replace(',', '.');
        else
            cleaned = cleaned.Replace(",", string.Empty);

        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    private static DateTime? ReadDate(JsonElement parent, string name)
    {
        var text = ReadString(parent, name);
        if (text is null) return null;
        if (DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return exact;
        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed.Date
            : null;
    }

    private static decimal? Abs(decimal? value) => value is null ? null : Math.Abs(value.Value);
}
