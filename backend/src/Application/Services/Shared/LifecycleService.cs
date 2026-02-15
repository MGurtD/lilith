using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Shared;

namespace Application.Services.Shared;

public class LifecycleService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : ILifecycleService
{
    // Lifecycle operations
    public async Task<GenericResponse> CreateLifecycle(Lifecycle lifecycle)
    {
        var exists = unitOfWork.Lifecycles.Find(r => lifecycle.Name == r.Name).Any();
        if (exists)
        {
            var message = localizationService.GetLocalizedString("EntityAlreadyExists");
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.Add(lifecycle);
        return new GenericResponse(true, lifecycle);
    }

    public async Task<IEnumerable<Lifecycle>> GetAllLifecycles()
    {
        var lifecycles = await unitOfWork.Lifecycles.GetAll();
        return lifecycles.OrderBy(e => e.Name);
    }

    public async Task<Lifecycle?> GetLifecycleById(Guid id)
    {
        return await unitOfWork.Lifecycles.Get(id);
    }

    public async Task<Lifecycle?> GetLifecycleByName(string name)
    {
        return await unitOfWork.Lifecycles.GetByName(name);
    }

    public async Task<GenericResponse> UpdateLifecycle(Lifecycle lifecycle)
    {
        var exists = await unitOfWork.Lifecycles.Exists(lifecycle.Id);
        if (!exists)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", lifecycle.Id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.Update(lifecycle);
        return new GenericResponse(true, lifecycle);
    }

    public async Task<GenericResponse> RemoveLifecycle(Guid id)
    {
        var entity = unitOfWork.Lifecycles.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.Remove(entity);
        return new GenericResponse(true, entity);
    }

    // Status operations
    public async Task<GenericResponse> CreateStatus(Status status)
    {
        var entity = await unitOfWork.Lifecycles.StatusRepository.Get(status.Id);
        if (entity is not null)
        {
            var message = localizationService.GetLocalizedString("EntityAlreadyExists");
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.Add(status);
        return new GenericResponse(true, status);
    }

    public async Task<Status?> GetStatusByName(string lifecycleName, string name)
    {
        return await unitOfWork.Lifecycles.GetStatusByName(lifecycleName, name);
    }

    public async Task<GenericResponse> UpdateStatus(Status status)
    {
        var exists = unitOfWork.Lifecycles.StatusRepository.Find(s => s.Id == status.Id).FirstOrDefault();
        if (exists is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", status.Id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.Update(status);
        return new GenericResponse(true, status);
    }

    public async Task<GenericResponse> RemoveStatus(Guid id)
    {
        var status = unitOfWork.Lifecycles.StatusRepository.Find(s => s.Id == id).FirstOrDefault();
        if (status is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.Remove(status);
        return new GenericResponse(true, status);
    }

    // StatusTransition operations
    public async Task<GenericResponse> CreateStatusTransition(StatusTransition transition)
    {
        if (!transition.StatusId.HasValue || !transition.StatusToId.HasValue)
        {
            var message = localizationService.GetLocalizedString("Validation.Required", "StatusId and StatusToId");
            return new GenericResponse(false, message);
        }

        var status = await unitOfWork.Lifecycles.StatusRepository.Get(transition.StatusId.Value);
        var statusTo = await unitOfWork.Lifecycles.StatusRepository.Get(transition.StatusToId.Value);
        
        if (status is null || statusTo is null)
        {
            var message = localizationService.GetLocalizedString("StatusNotFound", 
                status is null ? transition.StatusId.Value : transition.StatusToId.Value);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Add(transition);
        return new GenericResponse(true, transition);
    }

    public async Task<GenericResponse> UpdateStatusTransition(StatusTransition transition)
    {
        var existing = unitOfWork.Lifecycles.StatusRepository.TransitionRepository
            .Find(t => t.Id == transition.Id).FirstOrDefault();
        
        if (existing is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", transition.Id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Update(transition);
        return new GenericResponse(true, transition);
    }

    public async Task<GenericResponse> RemoveStatusTransition(Guid id)
    {
        var transition = unitOfWork.Lifecycles.StatusRepository.TransitionRepository
            .Find(t => t.Id == id).FirstOrDefault();
        
        if (transition is null)
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Remove(transition);
        return new GenericResponse(true, transition);
    }

    public async Task<IEnumerable<AvailableStatusTransitionDto>> GetAvailableTransitions(Guid statusId)
    {
        return await unitOfWork.Lifecycles.StatusRepository.GetAvailableTransitions(statusId);
    }

    // LifecycleTag operations
    public async Task<LifecycleTag?> GetTagById(Guid id)
    {
        return await unitOfWork.LifecycleTags.Get(id);
    }

    public async Task<IEnumerable<LifecycleTag>> GetTagsByLifecycle(Guid lifecycleId)
    {
        return await unitOfWork.LifecycleTags.GetByLifecycleId(lifecycleId);
    }

    public async Task<GenericResponse> CreateTag(Guid lifecycleId, LifecycleTag tag)
    {
        // Validate lifecycle exists
        var lifecycle = await unitOfWork.Lifecycles.Get(lifecycleId);
        if (lifecycle is null)
        {
            var message = localizationService.GetLocalizedString("LifecycleNotFound", lifecycleId);
            return new GenericResponse(false, message);
        }

        // Validate name is provided
        if (string.IsNullOrWhiteSpace(tag.Name))
        {
            var message = localizationService.GetLocalizedString("LifecycleTagNameRequired");
            return new GenericResponse(false, message);
        }

        // Check if name already exists in this lifecycle
        var exists = await unitOfWork.LifecycleTags.ExistsInLifecycle(tag.Name, lifecycleId);
        if (exists)
        {
            var message = localizationService.GetLocalizedString("LifecycleTagAlreadyExists", tag.Name);
            return new GenericResponse(false, message);
        }

        // Set lifecycle ID from parameter
        tag.LifecycleId = lifecycleId;
        
        await unitOfWork.LifecycleTags.Add(tag);
        return new GenericResponse(true, tag);
    }

    public async Task<GenericResponse> UpdateTag(LifecycleTag tag)
    {
        var existingTag = await unitOfWork.LifecycleTags.Get(tag.Id);
        if (existingTag is null)
        {
            var message = localizationService.GetLocalizedString("LifecycleTagNotFound", tag.Id);
            return new GenericResponse(false, message);
        }

        // Validate name is provided
        if (string.IsNullOrWhiteSpace(tag.Name))
        {
            var message = localizationService.GetLocalizedString("LifecycleTagNameRequired");
            return new GenericResponse(false, message);
        }

        // Check if name already exists (excluding current tag)
        var nameExists = await unitOfWork.LifecycleTags.ExistsInLifecycle(tag.Name, tag.LifecycleId, tag.Id);
        if (nameExists)
        {
            var message = localizationService.GetLocalizedString("LifecycleTagAlreadyExists", tag.Name);
            return new GenericResponse(false, message);
        }

        await unitOfWork.LifecycleTags.Update(tag);
        return new GenericResponse(true, tag);
    }

    public async Task<GenericResponse> RemoveTag(Guid id)
    {
        var tag = await unitOfWork.LifecycleTags.Get(id);
        if (tag is null)
        {
            var message = localizationService.GetLocalizedString("LifecycleTagNotFound", id);
            return new GenericResponse(false, message);
        }

        await unitOfWork.LifecycleTags.Remove(tag);
        return new GenericResponse(true, tag);
    }

    public async Task<IEnumerable<LifecycleTag>> GetTagsByStatus(Guid statusId)
    {
        return await unitOfWork.LifecycleTags.GetTagsByStatus(statusId);
    }

    public async Task<GenericResponse> AssignTagToStatus(Guid statusId, Guid tagId)
    {
        var status = await unitOfWork.Lifecycles.StatusRepository.Get(statusId);
        if (status is null)
        {
            var message = localizationService.GetLocalizedString("StatusNotFound", statusId);
            return new GenericResponse(false, message);
        }

        var tag = await unitOfWork.LifecycleTags.Get(tagId);
        if (tag is null)
        {
            var message = localizationService.GetLocalizedString("LifecycleTagNotFound", tagId);
            return new GenericResponse(false, message);
        }

        // Validate both belong to the same lifecycle
        if (status.LifecycleId != tag.LifecycleId)
        {
            var message = localizationService.GetLocalizedString("TagNotInSameLifecycle");
            return new GenericResponse(false, message);
        }

        // Check if already assigned
        var alreadyAssigned = unitOfWork.StatusLifecycleTags
            .Find(slt => slt.StatusId == statusId && slt.LifecycleTagId == tagId && !slt.Disabled)
            .Any();

        if (alreadyAssigned)
        {
            var message = localizationService.GetLocalizedString("LifecycleTagAssigned");
            return new GenericResponse(true, message);
        }

        // Create the assignment
        var assignment = new StatusLifecycleTag
        {
            StatusId = statusId,
            LifecycleTagId = tagId
        };

        await unitOfWork.StatusLifecycleTags.Add(assignment);

        var successMessage = localizationService.GetLocalizedString("LifecycleTagAssigned");
        return new GenericResponse(true, successMessage);
    }

    public async Task<GenericResponse> RemoveTagFromStatus(Guid statusId, Guid tagId)
    {
        var status = await unitOfWork.Lifecycles.StatusRepository.Get(statusId);
        if (status is null)
        {
            var message = localizationService.GetLocalizedString("StatusNotFound", statusId);
            return new GenericResponse(false, message);
        }

        var tag = await unitOfWork.LifecycleTags.Get(tagId);
        if (tag is null)
        {
            var message = localizationService.GetLocalizedString("LifecycleTagNotFound", tagId);
            return new GenericResponse(false, message);
        }

        // Find the assignment
        var assignment = unitOfWork.StatusLifecycleTags
            .Find(slt => slt.StatusId == statusId && slt.LifecycleTagId == tagId && !slt.Disabled)
            .FirstOrDefault();

        if (assignment is null)
        {
            var message = localizationService.GetLocalizedString("LifecycleTagNotFound", tagId);
            return new GenericResponse(false, message);
        }

        await unitOfWork.StatusLifecycleTags.Remove(assignment);

        var successMessage = localizationService.GetLocalizedString("LifecycleTagRemoved");
        return new GenericResponse(true, successMessage);
    }
}






