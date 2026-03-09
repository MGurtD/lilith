using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production;

public class PhaseTemplateService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IPhaseTemplateService
{
    #region Template CRUD

    public async Task<PhaseTemplate?> GetById(Guid id)
    {
        return await unitOfWork.PhaseTemplates.Get(id);
    }

    public async Task<IEnumerable<PhaseTemplate>> GetAll()
    {
        return await unitOfWork.PhaseTemplates.GetAll();
    }

    public async Task<GenericResponse> Create(PhaseTemplate template)
    {
        var exists = unitOfWork.PhaseTemplates.Find(t => t.Id == template.Id).Any();
        if (exists)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("PhaseTemplateAlreadyExists"));

        await unitOfWork.PhaseTemplates.Add(template);
        return new GenericResponse(true, template);
    }

    public async Task<GenericResponse> Update(PhaseTemplate template)
    {
        var exists = await unitOfWork.PhaseTemplates.Exists(template.Id);
        if (!exists)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("PhaseTemplateNotFound"));

        await unitOfWork.PhaseTemplates.Update(template);
        return new GenericResponse(true, template);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.PhaseTemplates.Find(t => t.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("PhaseTemplateNotFound"));

        await unitOfWork.PhaseTemplates.Remove(entity);
        return new GenericResponse(true, entity);
    }

    #endregion

    #region Detail CRUD

    public async Task<PhaseTemplateDetail?> GetDetailById(Guid id)
    {
        return await unitOfWork.PhaseTemplates.Details.Get(id);
    }

    public async Task<GenericResponse> CreateDetail(PhaseTemplateDetail detail)
    {
        var exists = unitOfWork.PhaseTemplates.Details.Find(d => d.Id == detail.Id).Any();
        if (exists)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("PhaseTemplateDetailAlreadyExists"));

        await unitOfWork.PhaseTemplates.Details.Add(detail);
        return new GenericResponse(true, detail);
    }

    public async Task<GenericResponse> UpdateDetail(PhaseTemplateDetail detail)
    {
        var exists = await unitOfWork.PhaseTemplates.Details.Exists(detail.Id);
        if (!exists)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("PhaseTemplateDetailNotFound"));

        await unitOfWork.PhaseTemplates.Details.Update(detail);
        return new GenericResponse(true, detail);
    }

    public async Task<GenericResponse> RemoveDetail(Guid id)
    {
        var entity = unitOfWork.PhaseTemplates.Details.Find(d => d.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("PhaseTemplateDetailNotFound"));

        await unitOfWork.PhaseTemplates.Details.Remove(entity);
        return new GenericResponse(true, entity);
    }

    #endregion
}
