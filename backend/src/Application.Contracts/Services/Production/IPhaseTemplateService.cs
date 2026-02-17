using Domain.Entities.Production;

namespace Application.Contracts;

public interface IPhaseTemplateService
{
    Task<PhaseTemplate?> GetById(Guid id);
    Task<IEnumerable<PhaseTemplate>> GetAll();
    Task<GenericResponse> Create(PhaseTemplate template);
    Task<GenericResponse> Update(PhaseTemplate template);
    Task<GenericResponse> Remove(Guid id);

    // Detail CRUD
    Task<PhaseTemplateDetail?> GetDetailById(Guid id);
    Task<GenericResponse> CreateDetail(PhaseTemplateDetail detail);
    Task<GenericResponse> UpdateDetail(PhaseTemplateDetail detail);
    Task<GenericResponse> RemoveDetail(Guid id);
}
