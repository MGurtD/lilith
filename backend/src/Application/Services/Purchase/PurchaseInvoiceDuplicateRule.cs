using System.Linq.Expressions;
using Domain.Entities.Purchase;

namespace Application.Services.Purchase;

/// <summary>
/// A purchase invoice duplicates another one when both belong to the same supplier and
/// carry the same supplier invoice number (ignoring case and surrounding spaces).
/// Empty numbers and the "--" placeholder used for unknown numbers are never duplicates.
/// </summary>
internal static class PurchaseInvoiceDuplicateRule
{
    private const string UnknownNumber = "--";

    public static bool Applies(Guid supplierId, string? supplierNumber) =>
        supplierId != Guid.Empty
        && !string.IsNullOrWhiteSpace(supplierNumber)
        && supplierNumber.Trim() != UnknownNumber;

    public static Expression<Func<PurchaseInvoice, bool>> SameSupplierAndNumber(
        Guid supplierId,
        string supplierNumber,
        Guid? excludedInvoiceId = null)
    {
        var number = supplierNumber.Trim().ToUpper();
        return p => p.SupplierId == supplierId
            && p.SupplierNumber.Trim().ToUpper() == number
            && (excludedInvoiceId == null || p.Id != excludedInvoiceId);
    }
}
