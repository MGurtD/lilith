using Application.Contracts;
using Domain.Entities;

namespace Application.Services.Shared;

public class PaymentMethodService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IPaymentMethodService
{
    public async Task<GenericResponse> CreatePaymentMethod(PaymentMethod paymentMethod)
    {
        var exists = unitOfWork.PaymentMethods.Find(e => e.Id == paymentMethod.Id).Any();
        if (exists)
        {
            var message = localizationService.GetLocalizedString("EntityAlreadyExists");
            return new GenericResponse(false, message);
        }

        await unitOfWork.PaymentMethods.Add(paymentMethod);
        return new GenericResponse(true, paymentMethod);
    }

    public async Task<IEnumerable<PaymentMethod>> GetAllPaymentMethods()
    {
        var entities = await unitOfWork.PaymentMethods.GetAll();
        return entities.OrderBy(e => e.Name);
    }

    public async Task<PaymentMethod?> GetPaymentMethodById(Guid id)
    {
        return await unitOfWork.PaymentMethods.Get(id);
    }

    public async Task<GenericResponse> UpdatePaymentMethod(PaymentMethod paymentMethod)
    {
        var exists = await unitOfWork.PaymentMethods.Exists(paymentMethod.Id);
        if (!exists)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", paymentMethod.Id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.PaymentMethods.Update(paymentMethod);
        return new GenericResponse(true, paymentMethod);
    }

    public async Task<GenericResponse> RemovePaymentMethod(Guid id)
    {
        var entity = unitOfWork.PaymentMethods.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.PaymentMethods.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
