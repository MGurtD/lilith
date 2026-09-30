using Domain.Entities;

namespace Application.Contracts;

public interface IPaymentMethodRepository : IRepository<PaymentMethod, Guid>
{
}
