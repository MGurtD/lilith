using Application.Contracts;
using Domain.Entities;

namespace Application.Tests.TestSupport;

/// <summary>
/// In-memory <see cref="ITaxRepository"/>; the ids in <c>taxesInUse</c> are
/// reported as used by references or invoices.
/// </summary>
public sealed class InMemoryTaxRepository(IEnumerable<Tax>? seed = null, IEnumerable<Guid>? taxesInUse = null)
    : InMemoryRepository<Tax>(seed), ITaxRepository
{
    private readonly HashSet<Guid> inUse = taxesInUse?.ToHashSet() ?? [];

    public Task<bool> IsInUse(Guid taxId) => Task.FromResult(inUse.Contains(taxId));
}
