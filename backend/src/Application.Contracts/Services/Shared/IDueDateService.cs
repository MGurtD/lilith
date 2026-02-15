using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Shared;

namespace Application.Contracts
{
    public interface IDueDateService
    {
        List<DueDate> GenerateDueDates(PaymentMethod paymentMethod, DateTime date, decimal totalAmount);
    }
}
