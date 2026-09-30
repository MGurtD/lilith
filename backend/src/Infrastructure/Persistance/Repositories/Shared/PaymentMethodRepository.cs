using Application.Contracts;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories;

public class PaymentMethodRepository(ApplicationDbContext context) : Repository<PaymentMethod, Guid>(context), IPaymentMethodRepository
{
}
