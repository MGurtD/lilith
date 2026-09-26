using Application.Contracts;
using Domain.Entities.Purchase;

namespace Application.Services.Purchase;

public class ExpenseService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IExpenseService
{
    public async Task<Expenses?> GetExpenseById(Guid id)
    {
        return await unitOfWork.Expenses.Get(id);
    }

    public IEnumerable<Expenses> GetBetweenDatesAndType(DateTime startTime, DateTime endTime, Guid? expenseTypeId)
    {
        var entities = unitOfWork.Expenses.Find(e => e.PaymentDate >= startTime && e.PaymentDate <= endTime);
        if (expenseTypeId.HasValue)
        {
            entities = entities.Where(e => e.ExpenseTypeId == expenseTypeId.Value);
        }
        return entities.OrderBy(e => e.PaymentDate);
    }

    public IEnumerable<Expenses> GetByType(Guid expenseTypeId)
    {
        return unitOfWork.Expenses.Find(p => p.ExpenseTypeId == expenseTypeId);
    }

    public IEnumerable<ConsolidatedExpense> GetConsolidatedBetweenDates(DateTime startTime, DateTime endTime)
    {
        return unitOfWork.ConsolidatedExpenses.Find(c => c.PaymentDate >= startTime && c.PaymentDate <= endTime);
    }

    public async Task<GenericResponse> Create(Expenses expense)
    {
        await unitOfWork.Expenses.Add(expense);
        await CreateRecurringExpenses(expense);

        return new GenericResponse(true, expense);
    }

    public async Task<GenericResponse> UpdateExpense(Expenses expense)
    {
        var exists = await unitOfWork.Expenses.Exists(expense.Id);
        if (!exists)
        {
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("EntityNotFound", expense.Id));
        }        

        await unitOfWork.Expenses.Update(expense);
        await RemoveRecurringExpenses(expense);
        await CreateRecurringExpenses(expense);
        return new GenericResponse(true, expense);
    }

    // Genera les instàncies futures d'una despesa recurrent; no fa res si l'entitat no és recurrent
    private async Task CreateRecurringExpenses(Expenses expense)
    {
        if (!expense.Recurring)
            return;

        expense.RelatedExpenseId = expense.Id.ToString();
        DateTime paymentDay = expense.PaymentDate;

        while (paymentDay < expense.EndDate)
        {
            paymentDay = paymentDay.AddMonths(expense.Frecuency);
            
            if (expense.PaymentDate.Day != expense.PaymentDay)
            {
                paymentDay = paymentDay.AddDays(expense.PaymentDay - expense.PaymentDate.Day);
            }

            var recurringExpense = new Expenses
            {
                Id = Guid.NewGuid(),
                PaymentDate = paymentDay,
                RelatedExpenseId = expense.RelatedExpenseId,
                ExpenseTypeId = expense.ExpenseTypeId,
                Amount = expense.Amount,
                Description = expense.Description,
                Recurring = expense.Recurring,
                Frecuency = expense.Frecuency,
                PaymentDay = expense.PaymentDay,
                EndDate = expense.EndDate
            };

            await unitOfWork.Expenses.Add(recurringExpense);
        }
    }

    public async Task<GenericResponse> RemoveExpense(Guid id)
    {
        var entity = unitOfWork.Expenses.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
        {
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("EntityNotFound", id));
        }

        if (entity.Recurring)
        {
            await RemoveRecurringExpenses(entity);
        }
        else
        {
            await unitOfWork.Expenses.Remove(entity);
        }

        return new GenericResponse(true, entity);
    }

    // Elimina la despesa pare i totes les instàncies relacionades; no fa res si l'entitat no és recurrent
    private async Task RemoveRecurringExpenses(Expenses expense)
    {
        if (!expense.Recurring)
            return;

        var parentId = string.IsNullOrEmpty(expense.RelatedExpenseId) ? expense.Id : Guid.Parse(expense.RelatedExpenseId);
        var relatedParent = unitOfWork.Expenses.Find(e => e.Id == parentId).FirstOrDefault();
        if (relatedParent is not null)
            await unitOfWork.Expenses.Remove(relatedParent);

        var relatedExpenses = unitOfWork.Expenses.Find(e => e.RelatedExpenseId == parentId.ToString());
        await unitOfWork.Expenses.RemoveRange(relatedExpenses);
    }
}
