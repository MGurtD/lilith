using Domain.Entities;
using Domain.Entities.Shared;

namespace Application.Contracts
{
    public interface IStatusRepository : IRepository<Status, Guid>
    {
        IRepository<StatusTransition, Guid> TransitionRepository { get; }

        Task AddTransition(StatusTransition transition);
        Task UpdateTransition(StatusTransition transition);
        Task<bool> RemoveTransition(StatusTransition transition);
        Task<IEnumerable<AvailableStatusTransitionDto>> GetAvailableTransitions(Guid statusId);
    }
}
