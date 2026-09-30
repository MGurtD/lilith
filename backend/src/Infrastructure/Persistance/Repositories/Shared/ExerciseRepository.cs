using Application.Contracts;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories;

public class ExerciseRepository(ApplicationDbContext context) : Repository<Exercise, Guid>(context), IExerciseRepository
{
}
