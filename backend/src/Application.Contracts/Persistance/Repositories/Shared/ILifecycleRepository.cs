using Domain.Entities;
using Domain.Entities.Shared;

namespace Application.Contracts
{
    public interface ILifecycleRepository : IRepository<Lifecycle, Guid>
    {
        IStatusRepository StatusRepository { get; }

        Task<Lifecycle?> GetByName(string code);
        Task<Status?> GetStatusByName(string lifecycleCode, string statusCode);
        Task<Guid?> GetInitialStatusByName(string name);
    }
}
