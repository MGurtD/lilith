using Domain.Entities;

namespace Application.Contracts;

public interface ITaxRepository : IRepository<Tax, Guid>
{
    /// <summary>
    /// True when a reference, a sales invoice line or an invoice tax breakdown
    /// (sales or purchase) uses the tax.
    /// </summary>
    Task<bool> IsInUse(Guid taxId);
}
