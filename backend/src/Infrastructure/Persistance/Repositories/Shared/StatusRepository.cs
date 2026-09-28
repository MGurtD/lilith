using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
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

        // Delivery notes, work orders, their phases, purchase orders and their lines
        // reference the status with a cascading foreign key, so deleting a status in
        // use would silently delete those documents.
        public async Task<bool> IsInUse(Guid statusId)
        {
            return await context.Set<DeliveryNote>().AnyAsync(e => e.StatusId == statusId)
                || await context.Set<WorkOrder>().AnyAsync(e => e.StatusId == statusId)
                || await context.Set<WorkOrderPhase>().AnyAsync(e => e.StatusId == statusId)
                || await context.Set<PurchaseOrder>().AnyAsync(e => e.StatusId == statusId)
                || await context.Set<PurchaseOrderDetail>().AnyAsync(e => e.StatusId == statusId)
                || await context.Set<Receipt>().AnyAsync(e => e.StatusId == statusId)
                || await context.Set<PurchaseInvoice>().AnyAsync(e => e.StatusId == statusId)
                || await context.Set<Budget>().AnyAsync(e => e.StatusId == statusId)
                || await context.Set<SalesOrderHeader>().AnyAsync(e => e.StatusId == statusId)
                || await context.Set<SalesInvoice>().AnyAsync(e => e.StatusId == statusId || e.IntegrationStatusId == statusId)
                || await context.Set<StatusTransition>().AnyAsync(e => e.StatusId == statusId || e.StatusToId == statusId)
                || await context.Set<Lifecycle>().AnyAsync(e => e.InitialStatusId == statusId || e.FinalStatusId == statusId);
        }
    }
}
