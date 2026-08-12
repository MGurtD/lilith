using Domain.Entities.Shared;

namespace Application.Contracts;

public interface IParameterService
{
    Task<IEnumerable<Parameter>> GetAll();
    Task<Parameter?> GetById(Guid id);
    Task<GenericResponse> Create(Parameter parameter);
    Task<GenericResponse> Update(Parameter parameter);
    Task<GenericResponse> Remove(Guid id);

    /// <summary>Llegeix un parametre booleà per clau; retorna defaultValue si no existeix.</summary>
    Task<bool> GetBool(string key, bool defaultValue);
}
