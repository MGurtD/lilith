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

        /// <summary>
        /// True when a document, a transition or a lifecycle's initial or final
        /// status uses the status. Tag assignments do not count.
        /// </summary>
        Task<bool> IsInUse(Guid statusId);
    }
}
