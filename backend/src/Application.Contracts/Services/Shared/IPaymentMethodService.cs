using Application.Contracts;
using Domain.Entities;

namespace Application.Contracts;

public interface IPaymentMethodService
{
    Task<GenericResponse> CreatePaymentMethod(PaymentMethod paymentMethod);
    Task<IEnumerable<PaymentMethod>> GetAllPaymentMethods();
    Task<PaymentMethod?> GetPaymentMethodById(Guid id);
    Task<GenericResponse> UpdatePaymentMethod(PaymentMethod paymentMethod);
    Task<GenericResponse> RemovePaymentMethod(Guid id);
}
