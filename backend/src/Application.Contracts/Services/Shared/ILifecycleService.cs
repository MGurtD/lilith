using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Shared;

namespace Application.Contracts;

public interface ILifecycleService
{
    // Lifecycle operations
    Task<GenericResponse> CreateLifecycle(Lifecycle lifecycle);
    Task<IEnumerable<Lifecycle>> GetAllLifecycles();
    Task<Lifecycle?> GetLifecycleById(Guid id);
    Task<Lifecycle?> GetLifecycleByName(string name);
    Task<GenericResponse> UpdateLifecycle(Lifecycle lifecycle);
    Task<GenericResponse> RemoveLifecycle(Guid id);

    // Status operations
    Task<GenericResponse> CreateStatus(Status status);
    Task<Status?> GetStatusByName(string lifecycleName, string name);
    Task<GenericResponse> UpdateStatus(Status status);
    Task<GenericResponse> RemoveStatus(Guid id);

    // StatusTransition operations
    Task<GenericResponse> CreateStatusTransition(StatusTransition transition);
    Task<GenericResponse> UpdateStatusTransition(StatusTransition transition);
    Task<GenericResponse> RemoveStatusTransition(Guid id);
    Task<IEnumerable<AvailableStatusTransitionDto>> GetAvailableTransitions(Guid statusId);

    // LifecycleTag operations
    Task<LifecycleTag?> GetTagById(Guid id);
    Task<IEnumerable<LifecycleTag>> GetTagsByLifecycle(Guid lifecycleId);
    Task<GenericResponse> CreateTag(Guid lifecycleId, LifecycleTag tag);
    Task<GenericResponse> UpdateTag(LifecycleTag tag);
    Task<GenericResponse> RemoveTag(Guid id);
    Task<IEnumerable<LifecycleTag>> GetTagsByStatus(Guid statusId);
    Task<GenericResponse> AssignTagToStatus(Guid statusId, Guid tagId);
    Task<GenericResponse> RemoveTagFromStatus(Guid statusId, Guid tagId);
}
