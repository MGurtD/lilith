using Application.Contracts;
using Application.Services;
using Domain.Entities.Sales;

namespace Application.Services.Sales;

public class CustomerTypeService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : ICustomerTypeService
{
    public async Task<IEnumerable<CustomerType>> GetAllCustomerTypes()
    {
        var customerTypes = await unitOfWork.CustomerTypes.GetAll();
        return customerTypes.OrderBy(ct => ct.Name);
    }

    public async Task<CustomerType?> GetCustomerTypeById(Guid id)
    {
        return await unitOfWork.CustomerTypes.Get(id);
    }

    public async Task<GenericResponse> CreateCustomerType(CustomerType customerType)
    {
        var exists = unitOfWork.CustomerTypes
            .Find(ct => ct.Name == customerType.Name)
            .Any();

        if (exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityAlreadyExists"));
        }

        await unitOfWork.CustomerTypes.Add(customerType);
        return new GenericResponse(true, customerType);
    }

    public async Task<GenericResponse> UpdateCustomerType(CustomerType customerType)
    {
        var exists = await unitOfWork.CustomerTypes.Get(customerType.Id);
        if (exists == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", customerType.Id));
        }

        await unitOfWork.CustomerTypes.Update(customerType);
        return new GenericResponse(true, customerType);
    }

    public async Task<GenericResponse> RemoveCustomerType(Guid id)
    {
        var customerType = unitOfWork.CustomerTypes.Find(ct => ct.Id == id).FirstOrDefault();
        if (customerType == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.CustomerTypes.Remove(customerType);
        return new GenericResponse(true, customerType);
    }
}
