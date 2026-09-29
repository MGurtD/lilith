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
        await CreateRecurringExpenses(expense, SeriesId(expense));

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
        if (expense.Recurring)
        {
            // The edited expense and the earlier instances of its series are kept;
            // only the following instances are regenerated from the edited values.
            var seriesId = SeriesId(expense);
            var following = unitOfWork.Expenses
                .Find(e => e.RelatedExpenseId == seriesId && e.Id != expense.Id && e.PaymentDate > expense.PaymentDate)
                .ToList();
            await unitOfWork.Expenses.RemoveRange(following);
            await CreateRecurringExpenses(expense, seriesId);
        }
        return new GenericResponse(true, expense);
    }

    // A series is identified by its first expense; every generated instance points to it
    // through RelatedExpenseId, while the first expense itself leaves it empty.
    private static string SeriesId(Expenses expense) =>
        string.IsNullOrEmpty(expense.RelatedExpenseId) ? expense.Id.ToString() : expense.RelatedExpenseId;

    // Generates the instances that follow a recurring expense, up to its end date inclusive.
    // Each date is computed from the base payment date so the payment day never drifts.
    private async Task CreateRecurringExpenses(Expenses expense, string seriesId)
    {
        if (!expense.Recurring || expense.Frecuency < 1)
            return;

        for (var period = 1; ; period++)
        {
            var paymentDate = RecurringPaymentDate(expense, period);
            if (paymentDate > expense.EndDate)
                break;

            var recurringExpense = new Expenses
            {
                Id = Guid.NewGuid(),
                PaymentDate = paymentDate,
                RelatedExpenseId = seriesId,
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

    // Payment date of the n-th following instance: the base date moved n periods ahead,
    // on the configured payment day (clamped to the month's length) when there is one.
    private static DateTime RecurringPaymentDate(Expenses expense, int period)
    {
        var date = expense.PaymentDate.AddMonths(expense.Frecuency * period);
        if (expense.PaymentDay < 1)
            return date;

        var day = Math.Min(expense.PaymentDay, DateTime.DaysInMonth(date.Year, date.Month));
        return date.AddDays(day - date.Day);
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

        var seriesId = SeriesId(expense);
        var parentId = Guid.Parse(seriesId);
        var relatedParent = unitOfWork.Expenses.Find(e => e.Id == parentId).FirstOrDefault();
        if (relatedParent is not null)
            await unitOfWork.Expenses.Remove(relatedParent);

        var relatedExpenses = unitOfWork.Expenses.Find(e => e.RelatedExpenseId == seriesId).ToList();
        await unitOfWork.Expenses.RemoveRange(relatedExpenses);
    }
}
