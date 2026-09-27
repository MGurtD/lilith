using Application.Contracts;
using Application.Contracts.Ingestion;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Purchase;

/// <summary>
/// Uninvoiced receipts of a supplier offered for linking to a purchase invoice, and the
/// rule that pre-selects the ones the invoice covers.
/// </summary>
internal static class ReceiptCandidates
{
    private const decimal AmountTolerance = 0.05m;
    // 2^12 subsets at most; beyond that only single receipts and all of them are tried.
    private const int MaxReceiptsForCombinations = 12;

    public static async Task<List<ReceiptCandidate>> LoadAsync(IUnitOfWork unitOfWork, Guid supplierId)
    {
        var receipts = await unitOfWork.Receipts.FindAsyncWithQueryParams(
            r => r.SupplierId == supplierId && r.PurchaseInvoiceId == null && !r.Disabled,
            query => query.Include(r => r.Details).AsNoTracking());

        return receipts
            .OrderBy(r => r.Date)
            .Select(r => new ReceiptCandidate
            {
                Id = r.Id,
                Number = r.Number,
                SupplierNumber = r.SupplierNumber,
                Date = r.Date,
                Amount = r.Details.Where(d => !d.Disabled).Sum(d => d.Amount),
            })
            .ToList();
    }

    /// <summary>
    /// Suggests the receipts whose delivery-note number is printed on the invoice. When none
    /// matches, suggests the only combination of receipts that adds up to the taxable base;
    /// several matching combinations are ambiguous and suggest nothing.
    /// </summary>
    public static void Suggest(
        IReadOnlyList<ReceiptCandidate> candidates,
        IEnumerable<string> deliveryNoteNumbers,
        decimal? taxableBase)
    {
        var printed = deliveryNoteNumbers
            .Select(Normalize)
            .Where(n => n.Length > 0)
            .ToHashSet();

        var byNumber = candidates
            .Where(c => printed.Contains(Normalize(c.SupplierNumber)) || printed.Contains(Normalize(c.Number)))
            .ToList();
        if (byNumber.Count > 0)
        {
            Mark(byNumber, ReceiptMatchReasons.DeliveryNoteNumber);
            return;
        }

        if (taxableBase is not > 0m || candidates.Count == 0) return;

        var match = FindSingleCombination(candidates, taxableBase.Value);
        if (match != null) Mark(match, ReceiptMatchReasons.Amount);
    }

    private static List<ReceiptCandidate>? FindSingleCombination(
        IReadOnlyList<ReceiptCandidate> candidates,
        decimal target)
    {
        IEnumerable<List<ReceiptCandidate>> combinations = candidates.Count <= MaxReceiptsForCombinations
            ? AllCombinations(candidates)
            : candidates.Select(c => new List<ReceiptCandidate> { c }).Append(candidates.ToList());

        List<ReceiptCandidate>? found = null;
        foreach (var combination in combinations)
        {
            if (Math.Abs(combination.Sum(c => c.Amount) - target) > AmountTolerance) continue;
            if (found != null) return null;
            found = combination;
        }
        return found;
    }

    private static IEnumerable<List<ReceiptCandidate>> AllCombinations(IReadOnlyList<ReceiptCandidate> candidates)
    {
        for (var mask = 1; mask < 1 << candidates.Count; mask++)
        {
            var combination = new List<ReceiptCandidate>();
            for (var bit = 0; bit < candidates.Count; bit++)
            {
                if ((mask & (1 << bit)) != 0) combination.Add(candidates[bit]);
            }
            yield return combination;
        }
    }

    private static void Mark(IEnumerable<ReceiptCandidate> receipts, string reason)
    {
        foreach (var receipt in receipts)
        {
            receipt.Suggested = true;
            receipt.MatchReason = reason;
        }
    }

    /// <summary>"A2025/4501", "a2025-4501" and "A 2025 4501" compare equal.</summary>
    internal static string Normalize(string? number) =>
        number == null ? string.Empty : new string(number.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
}
