using Application.Contracts;
using Application.Contracts.Ingestion;
using Application.Contracts.Persistance;
using Domain.Entities;

namespace Application.Ingestion;

/// <summary>
/// Maps the LlamaParse structured extraction response to the application DTO
/// and resolves tax rows against the local Tax catalog.
/// Anti-create policy: never inserts Tax rows; throws UnknownTaxRate on miss.
/// Surcharge policy: any row with a non-null surchargeRate triggers 422 immediately.
/// </summary>
public class LlamaParsePayloadMapper
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;

    public LlamaParsePayloadMapper(
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService)
    {
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
    }

    internal IngestPurchaseInvoiceResponse Map(LlamaParseExtractionData? data)
    {
        var response = new IngestPurchaseInvoiceResponse();

        if (data == null)
            return response;

        response.InvoiceNumber = data.InvoiceNumber;
        response.IssueDate = data.IssueDate;
        response.SupplierVatNumber = data.Supplier?.VatNumber;
        response.SupplierName = data.Supplier?.Name;
        response.BaseAmount = data.Totals?.BaseAmount;
        response.TransportAmount = data.Totals?.TransportAmount;
        response.DiscountPercentage = data.Totals?.DiscountPercentage;
        response.ExtraTaxPercentatge = data.Totals?.ExtraTaxPercentage;

        if (data.Confidence != null)
        {
            response.Confidence.Headers = data.Confidence.Headers;
            response.Confidence.Lines = data.Confidence.Lines;
        }

        // Surcharge / RE / IRPF detection.
        // Fires if any tax row has a non-zero surcharge_rate or surcharge_amount,
        // OR if the header totals indicate a non-zero extra tax (IRPF retention).
        // Both are conceptually "out-of-scope retention" patterns that this MVP does not persist.
        var hasRecargo = data.TaxBreakdown.Any(r =>
            (r.SurchargeRate.HasValue && r.SurchargeRate.Value > 0) ||
            (r.SurchargeAmount.HasValue && r.SurchargeAmount.Value > 0));
        var hasIrpf = (data.Totals?.ExtraTaxPercentage ?? 0) > 0;

        if (hasRecargo || hasIrpf)
        {
            throw new IngestionException(
                IngestionFailureKind.SurchargeUnsupported,
                _localizationService.GetLocalizedString("SurchargeNotSupportedInScope"));
        }

        var unknownRates = new List<decimal>();

        foreach (var row in data.TaxBreakdown)
        {
            // Tax catalog resolution by exact Percentatge match.
            var tax = _unitOfWork.Taxes
                .Find(t => t.Percentatge == row.TaxRate)
                .FirstOrDefault();

            if (tax == null)
            {
                unknownRates.Add(row.TaxRate);
                continue;
            }

            response.TaxBreakdown.Add(new TaxBreakdownRow
            {
                TaxRate = row.TaxRate,
                BaseAmount = row.BaseAmount,
                TaxAmount = row.TaxAmount,
                TaxId = tax.Id,
                Confidence = row.Confidence ?? 0m,
            });
        }

        if (unknownRates.Count > 0)
        {
            var ratesText = string.Join(", ", unknownRates.Select(r => r.ToString()));
            throw new IngestionException(
                IngestionFailureKind.UnknownTaxRate,
                _localizationService.GetLocalizedString("UnknownTaxRate", ratesText),
                unknownRates);
        }

        return response;
    }
}