using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class StatusRepository(ApplicationDbContext context) : Repository<Status, Guid>(context), IStatusRepository
    {
        public IRepository<StatusTransition, Guid> TransitionRepository { get; } = new Repository<StatusTransition, Guid>(context);

        public override async Task<Status?> Get(Guid id)
        {
            return await dbSet
                .AsNoTracking()
                .Include(d => d.Transitions)
                .Include(d => d.LifecycleTags)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddTransition(StatusTransition transition)
        {
            await TransitionRepository.Add(transition);
        }

        public async Task UpdateTransition(StatusTransition transition)
        {
            await TransitionRepository.Update(transition);
        }

        public async Task<bool> RemoveTransition(StatusTransition transition)
        {
            await TransitionRepository.Remove(transition);
            return true;
        }

        public async Task<IEnumerable<AvailableStatusTransitionDto>> GetAvailableTransitions(Guid statusId)
        {
            var query = from st in context.Set<StatusTransition>()
                        join s in context.Set<Status>() on st.StatusToId equals s.Id
                        where st.StatusId == statusId && !st.Disabled && !s.Disabled
                        select new AvailableStatusTransitionDto(
                            s.Id,
                            s.Name,
                            s.Description,
                            s.Color,
                            st.Name
                        );

            return await query.ToListAsync();
        }
    }
}
