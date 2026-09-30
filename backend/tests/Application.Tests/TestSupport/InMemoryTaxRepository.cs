using Application.Contracts;
using Domain.Entities;

namespace Application.Tests.TestSupport;

/// <summary>In-memory <see cref="ITaxRepository"/>.</summary>
public sealed class InMemoryTaxRepository(IEnumerable<Tax>? seed = null)
    : InMemoryRepository<Tax>(seed), ITaxRepository
{
}
