using System.Globalization;
using Application.Contracts;
using Application.Contracts.Ingestion;
using Application.Utils;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Application.Services.Purchase;

/// <summary>
/// Builds a draft purchase invoice from a supplier invoice PDF: extracts it with the
/// configured provider, resolves supplier and taxes against the catalogs and flags every
/// value the operator must review. Nothing is persisted.
/// </summary>
public class InvoiceIngestionService(
    IInvoiceExtractor extractor,
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService,
    IOptions<AppSettings> options) : IInvoiceIngestionService
{
    private const decimal RowTolerance = 0.02m;
    private const decimal TotalTolerance = 0.05m;
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

    private decimal LowConfidenceThreshold =>
        options.Value.Ingestion?.LowConfidenceThreshold ?? 0.8m;

    public async Task<IngestPurchaseInvoiceResponse> IngestAsync(
        Stream pdfStream,
        string fileName,
        CancellationToken ct = default)
    {
        using var buffer = new MemoryStream();
        await pdfStream.CopyToAsync(buffer, ct);
        if (!HasPdfSignature(buffer))
        {
            throw new IngestionException(
                IngestionFailureKind.InvalidFile,
                localizationService.GetLocalizedString("InvoiceIngestionInvalidFile"));
        }

        buffer.Position = 0;
        var extracted = await extractor.ExtractAsync(buffer, fileName, ct);

        var response = new IngestPurchaseInvoiceResponse
        {
            SupplierVatNumber = extracted.SupplierVatNumber,
            SupplierName = extracted.SupplierName,
            InvoiceNumber = extracted.InvoiceNumber,
            IssueDate = extracted.IssueDate,
            TotalAmount = extracted.TotalAmount,
        };

        CheckRequiredHeader(extracted, response);
        await ResolveSupplier(extracted, response);
        await ResolveTaxRows(extracted, response);
        ResolveWithholding(extracted, response);
        CheckTotal(extracted, response);
        FlagLowConfidence(extracted, response);

        return response;
    }

    private static bool HasPdfSignature(MemoryStream buffer)
    {
        if (buffer.Length < PdfSignature.Length) return false;
        return buffer.GetBuffer().AsSpan(0, PdfSignature.Length).SequenceEqual(PdfSignature);
    }

    private void CheckRequiredHeader(ExtractedInvoice extracted, IngestPurchaseInvoiceResponse response)
    {
        if (string.IsNullOrWhiteSpace(extracted.InvoiceNumber))
            AddIssue(response, IngestionIssueFields.SupplierNumber, IngestionIssueCodes.MissingValue);
        if (extracted.IssueDate is null)
            AddIssue(response, IngestionIssueFields.PurchaseInvoiceDate, IngestionIssueCodes.MissingValue);
        if (extracted.TotalAmount is null)
            AddIssue(response, IngestionIssueFields.NetAmount, IngestionIssueCodes.MissingValue);
        if (extracted.TaxRows.Count == 0)
            AddIssue(response, IngestionIssueFields.TaxBreakdown, IngestionIssueCodes.MissingValue);
    }

    private async Task ResolveSupplier(ExtractedInvoice extracted, IngestPurchaseInvoiceResponse response)
    {
        var vatNumber = NormalizeVatNumber(extracted.SupplierVatNumber);
        if (string.IsNullOrEmpty(vatNumber))
        {
            AddIssue(response, IngestionIssueFields.SupplierId, IngestionIssueCodes.MissingValue);
            return;
        }

        // Foreign VAT numbers are legitimate; only flag values that look Spanish but fail the checksum.
        if (LooksSpanish(vatNumber) && !SpanishFiscalIdValidator.IsValidSpanishFiscalId(vatNumber))
        {
            AddIssue(response, IngestionIssueFields.SupplierId, IngestionIssueCodes.InvalidVatNumber,
                args: extracted.SupplierVatNumber!);
        }

        var suppliers = await unitOfWork.Suppliers.FindAsync(s => !s.Disabled);
        var matches = suppliers
            .Where(s => NormalizeVatNumber(s.VatNumber) == vatNumber)
            .ToList();

        switch (matches.Count)
        {
            case 1:
                response.SupplierId = matches[0].Id;
                break;
            case 0:
                AddIssue(response, IngestionIssueFields.SupplierId, IngestionIssueCodes.SupplierNotFound,
                    args: extracted.SupplierVatNumber!);
                break;
            default:
                AddIssue(response, IngestionIssueFields.SupplierId, IngestionIssueCodes.SupplierAmbiguous,
                    args: [extracted.SupplierVatNumber!, matches.Count]);
                break;
        }
    }

    private async Task ResolveTaxRows(ExtractedInvoice extracted, IngestPurchaseInvoiceResponse response)
    {
        var taxes = await unitOfWork.Taxes.FindAsync(t => !t.Disabled);

        for (var index = 0; index < extracted.TaxRows.Count; index++)
        {
            var row = extracted.TaxRows[index];
            var baseAmount = row.BaseAmount ?? 0m;
            var taxRate = row.TaxRate ?? InferRate(baseAmount, row.TaxAmount);
            var taxAmount = row.TaxAmount ?? Math.Round(baseAmount * taxRate / 100m, 2);

            var resolved = new TaxBreakdownRow
            {
                TaxRate = taxRate,
                BaseAmount = baseAmount,
                TaxAmount = taxAmount,
                SurchargeRate = row.SurchargeRate,
                SurchargeAmount = row.SurchargeAmount,
                TaxId = ResolveTax(taxes, taxRate, index, response),
            };
            response.TaxBreakdown.Add(resolved);

            if (row.BaseAmount is null || row.TaxAmount is null)
            {
                AddIssue(response, IngestionIssueFields.TaxBreakdown, IngestionIssueCodes.MissingValue, index);
            }
            else
            {
                var expected = Math.Round(baseAmount * taxRate / 100m, 2);
                if (Math.Abs(expected - taxAmount) > RowTolerance)
                {
                    AddIssue(response, IngestionIssueFields.TaxBreakdown, IngestionIssueCodes.TaxAmountMismatch,
                        index, Money(expected), Money(taxAmount));
                }
            }

            if ((row.SurchargeRate ?? 0m) > 0m || (row.SurchargeAmount ?? 0m) > 0m)
            {
                AddIssue(response, IngestionIssueFields.TaxBreakdown, IngestionIssueCodes.SurchargeNotImported,
                    index, Percent(row.SurchargeRate ?? 0m), Money(row.SurchargeAmount ?? 0m));
            }
        }
    }

    private Guid? ResolveTax(
        IReadOnlyCollection<Tax> taxes,
        decimal taxRate,
        int index,
        IngestPurchaseInvoiceResponse response)
    {
        var matches = taxes.Where(t => t.Percentatge == taxRate).ToList();
        // A 0% rate can be both an exemption and a reverse charge; prefer the plain tax.
        if (matches.Count > 1)
            matches = matches.Where(t => !t.IsReverseCharge).ToList();

        if (matches.Count == 1) return matches[0].Id;

        var code = matches.Count == 0
            ? IngestionIssueCodes.UnknownTaxRate
            : IngestionIssueCodes.AmbiguousTaxRate;
        AddIssue(response, IngestionIssueFields.TaxBreakdown, code, index, Percent(taxRate));
        return null;
    }

    private static void ResolveWithholding(ExtractedInvoice extracted, IngestPurchaseInvoiceResponse response)
    {
        if (extracted.WithholdingPercentage is > 0m)
        {
            response.ExtraTaxPercentatge = extracted.WithholdingPercentage;
            return;
        }

        var taxableBase = response.TaxBreakdown.Sum(r => r.BaseAmount);
        if (extracted.WithholdingAmount is > 0m && taxableBase > 0m)
            response.ExtraTaxPercentatge = Math.Round(extracted.WithholdingAmount.Value / taxableBase * 100m, 2);
    }

    /// <summary>
    /// Compares the printed total with the one the purchase invoice form will compute:
    /// bases + taxes − withholding over the bases. The surcharge is added back because it is
    /// part of the printed total but is not imported (it already carries its own issue).
    /// </summary>
    private void CheckTotal(ExtractedInvoice extracted, IngestPurchaseInvoiceResponse response)
    {
        if (extracted.TotalAmount is null || response.TaxBreakdown.Count == 0) return;

        var taxableBase = response.TaxBreakdown.Sum(r => r.BaseAmount);
        var taxes = response.TaxBreakdown.Sum(r => r.TaxAmount);
        var surcharge = response.TaxBreakdown.Sum(r => r.SurchargeAmount ?? 0m);
        var withholding = taxableBase * (response.ExtraTaxPercentatge ?? 0m) / 100m;
        var computed = Math.Round(taxableBase + taxes + surcharge - withholding, 2);

        if (Math.Abs(computed - extracted.TotalAmount.Value) > TotalTolerance)
        {
            AddIssue(response, IngestionIssueFields.NetAmount, IngestionIssueCodes.TotalMismatch,
                args: [Money(computed), Money(extracted.TotalAmount.Value)]);
        }
    }

    private void FlagLowConfidence(ExtractedInvoice extracted, IngestPurchaseInvoiceResponse response)
    {
        var headerFields = new (string Source, string Target)[]
        {
            (ExtractedInvoiceFields.InvoiceNumber, IngestionIssueFields.SupplierNumber),
            (ExtractedInvoiceFields.IssueDate, IngestionIssueFields.PurchaseInvoiceDate),
            (ExtractedInvoiceFields.SupplierVatNumber, IngestionIssueFields.SupplierId),
            (ExtractedInvoiceFields.WithholdingPercentage, IngestionIssueFields.ExtraTaxPercentatge),
            (ExtractedInvoiceFields.WithholdingAmount, IngestionIssueFields.ExtraTaxPercentatge),
            (ExtractedInvoiceFields.TotalAmount, IngestionIssueFields.NetAmount),
        };

        foreach (var group in headerFields.GroupBy(f => f.Target))
        {
            var lowest = group
                .Select(f => extracted.FieldConfidence.TryGetValue(f.Source, out var c) ? c : (decimal?)null)
                .Where(c => c.HasValue)
                .Min();
            if (lowest < LowConfidenceThreshold)
                AddIssue(response, group.Key, IngestionIssueCodes.LowConfidence, args: Percent(lowest.Value * 100m));
        }

        string[] rowFields = [ExtractedInvoiceFields.TaxRate, ExtractedInvoiceFields.BaseAmount, ExtractedInvoiceFields.TaxAmount];
        for (var index = 0; index < extracted.TaxRows.Count; index++)
        {
            var lowest = rowFields
                .Select(f => extracted.FieldConfidence.TryGetValue(ExtractedInvoiceFields.TaxRow(index, f), out var c) ? c : (decimal?)null)
                .Where(c => c.HasValue)
                .Min();
            if (lowest < LowConfidenceThreshold)
                AddIssue(response, IngestionIssueFields.TaxBreakdown, IngestionIssueCodes.LowConfidence,
                    index, Percent(lowest.Value * 100m));
        }
    }

    private void AddIssue(
        IngestPurchaseInvoiceResponse response,
        string field,
        string code,
        int? rowIndex = null,
        params object[] args)
    {
        response.Issues.Add(new IngestionIssue
        {
            Field = field,
            RowIndex = rowIndex,
            Code = code,
            Message = localizationService.GetLocalizedString($"InvoiceIngestion{code}", args),
        });
    }

    private static decimal InferRate(decimal baseAmount, decimal? taxAmount) =>
        baseAmount == 0m || taxAmount is null ? 0m : Math.Round(taxAmount.Value / baseAmount * 100m, 2);

    private static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Percent(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// Uppercases and strips separators and a leading ES country prefix so that
    /// "ES-B12.345.678" and "b12345678" compare equal.
    /// </summary>
    internal static string NormalizeVatNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var normalized = new string(value.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        return normalized.Length == 11 && normalized.StartsWith("ES", StringComparison.Ordinal)
            ? normalized[2..]
            : normalized;
    }

    private static bool LooksSpanish(string normalizedVat) =>
        normalizedVat.Length == 9 && (char.IsDigit(normalizedVat[0]) || char.IsDigit(normalizedVat[1]));
}
