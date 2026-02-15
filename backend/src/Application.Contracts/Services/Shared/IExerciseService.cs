using Application.Contracts;
using Domain.Entities;

namespace Application.Contracts;

public interface IExerciseService
{
    Task<GenericResponse> GetNextCounter(Guid exerciseId, string counterName);
    Exercise? GetExerciceByDate(DateTime dateTime);
    
    // CRUD operations
    Task<Exercise?> GetById(Guid id);
    Task<IEnumerable<Exercise>> GetAll();
    Task<GenericResponse> Create(Exercise exercise);
    Task<GenericResponse> Update(Exercise exercise);
    Task<GenericResponse> Remove(Guid id);
}
