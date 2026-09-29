using Domain.Entities;

namespace Application.Contracts;

public interface IExerciseRepository : IRepository<Exercise, Guid>
{
    /// <summary>
    /// True when a document of any kind (budget, sales order, delivery note,
    /// sales invoice, purchase order, receipt, purchase invoice or work order)
    /// belongs to the exercise.
    /// </summary>
    Task<bool> IsInUse(Guid exerciseId);
}
