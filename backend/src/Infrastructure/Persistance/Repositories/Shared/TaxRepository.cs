using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories;

public class TaxRepository(ApplicationDbContext context) : Repository<Tax, Guid>(context), ITaxRepository
{
    // The invoice tax breakdowns reference the tax with a cascading foreign key,
    // so deleting a tax in use would silently delete them.
    public async Task<bool> IsInUse(Guid taxId)
    {
        return await context.Set<Reference>().AnyAsync(r => r.TaxId == taxId)
            || await context.Set<SalesInvoiceDetail>().AnyAsync(d => d.TaxId == taxId)
            || await context.Set<SalesInvoiceImport>().AnyAsync(i => i.TaxId == taxId)
            || await context.Set<PurchaseInvoiceImport>().AnyAsync(i => i.TaxId == taxId);
    }
}
