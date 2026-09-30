using Domain.Entities;

namespace Application.Contracts;

public interface IExerciseRepository : IRepository<Exercise, Guid>
{
}
