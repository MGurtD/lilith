using Domain.Entities;

namespace Application.Contracts;

public interface IPaymentMethodRepository : IRepository<PaymentMethod, Guid>
{
    /// <summary>
    /// True when a sales or purchase invoice, a customer or a supplier uses the
    /// payment method.
    /// </summary>
    Task<bool> IsInUse(Guid paymentMethodId);
}
