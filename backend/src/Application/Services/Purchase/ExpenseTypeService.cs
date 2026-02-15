using Application.Contracts;
using Application.Services;
using Domain.Entities.Purchase;

namespace Application.Services.Purchase;

public class ExpenseTypeService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService) : IExpenseTypeService
{
    public async Task<IEnumerable<ExpenseType>> GetAllExpenseTypes()
    {
        var entities = await unitOfWork.ExpenseTypes.GetAll();
        return entities.OrderBy(e => e.Name);
    }

    public async Task<ExpenseType?> GetExpenseTypeById(Guid id)
    {
        return await unitOfWork.ExpenseTypes.Get(id);
    }

    public async Task<GenericResponse> CreateExpenseType(ExpenseType expenseType)
    {
        var exists = unitOfWork.ExpenseTypes.Find(r => r.Name == expenseType.Name).Any();
        if (exists)
        {
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("EntityAlreadyExists"));
        }

        await unitOfWork.ExpenseTypes.Add(expenseType);
        return new GenericResponse(true, expenseType);
    }

    public async Task<GenericResponse> UpdateExpenseType(ExpenseType expenseType)
    {
        var exists = await unitOfWork.ExpenseTypes.Exists(expenseType.Id);
        if (!exists)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", expenseType.Id));
        }

        await unitOfWork.ExpenseTypes.Update(expenseType);
        return new GenericResponse(true, expenseType);
    }

    public async Task<GenericResponse> RemoveExpenseType(Guid id)
    {
        var entity = unitOfWork.ExpenseTypes.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        await unitOfWork.ExpenseTypes.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
