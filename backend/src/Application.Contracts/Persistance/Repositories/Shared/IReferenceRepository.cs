using Domain.Entities.Shared;

namespace Application.Contracts;

/// <summary>
/// What uses a reference. Each flag maps to one line of the delete refusal message.
/// </summary>
[Flags]
public enum ReferenceUsage
{
    None = 0,
    SalesOrders = 1 << 0,
    PurchaseOrders = 1 << 1,
    DeliveryNotes = 1 << 2,
    Budgets = 1 << 3,
    Receipts = 1 << 4,
    Stock = 1 << 5,
    StockMovements = 1 << 6,
    ProductionRoute = 1 << 7,
    WorkOrders = 1 << 8,
    BillOfMaterials = 1 << 9,
    ExternalServicePhases = 1 << 10,
    PurchaseRates = 1 << 11,
}

public interface IReferenceRepository : IRepository<Reference, Guid>
{
    /// <summary>
    /// The documents, stock and production data that use the reference.
    /// Supplier reference links do not count.
    /// </summary>
    Task<ReferenceUsage> GetUsage(Guid referenceId);
}
