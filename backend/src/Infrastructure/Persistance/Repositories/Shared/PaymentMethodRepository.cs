using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories;

public class PaymentMethodRepository(ApplicationDbContext context) : Repository<PaymentMethod, Guid>(context), IPaymentMethodRepository
{
    // Sales and purchase invoices reference the payment method with a cascading
    // foreign key, so deleting a payment method in use would silently delete them;
    // customers and suppliers would block the delete.
    public async Task<bool> IsInUse(Guid paymentMethodId)
    {
        return await context.Set<SalesInvoice>().AnyAsync(i => i.PaymentMethodId == paymentMethodId)
            || await context.Set<PurchaseInvoice>().AnyAsync(i => i.PaymentMethodId == paymentMethodId)
            || await context.Set<Customer>().AnyAsync(c => c.PaymentMethodId == paymentMethodId)
            || await context.Set<Supplier>().AnyAsync(s => s.PaymentMethodId == paymentMethodId);
    }
}
