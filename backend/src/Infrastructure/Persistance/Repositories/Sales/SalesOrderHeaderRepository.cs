using Application.Contracts;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories.Sales
{
    public class SalesOrderHeaderRepository : Repository<SalesOrderHeader, Guid>, ISalesOrderHeaderRepository
    {
        private readonly ISalesOrderDetailRepository _salesOrderDetailRepository;
        public IRepository<SalesOrderTransport, Guid> Transports { get; }
        public IRepository<SalesOrderExternalServices, Guid> ExternalServices { get; }
        public IRepository<SalesOrderExternalServiceDetail, Guid> ExternalServiceDetails { get; }
        public IRepository<SalesOrderDetailPhaseProfit, Guid> DetailPhaseProfits { get; }

        public SalesOrderHeaderRepository(ApplicationDbContext context, ISalesOrderDetailRepository salesOrderDetailRepository) : base(context)
        {
            _salesOrderDetailRepository = salesOrderDetailRepository;
            Transports = new Repository<SalesOrderTransport, Guid>(context);
            ExternalServices = new Repository<SalesOrderExternalServices, Guid>(context);
            ExternalServiceDetails = new Repository<SalesOrderExternalServiceDetail, Guid>(context);
            DetailPhaseProfits = new Repository<SalesOrderDetailPhaseProfit, Guid>(context);
        }

        public override async Task<SalesOrderHeader?> Get(Guid id)
        {
            var salesOrder = 
                await dbSet
                    .Include("SalesOrderDetails.Reference")
                    .Include("SalesOrderDetails.PhaseProfits")
                    .Include(b => b.Transports)
                    .Include(b => b.ExternalServices).ThenInclude(es => es.Details)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id);

            if (salesOrder != null && salesOrder.SalesOrderDetails.Any())
            {
                salesOrder.SalesOrderDetails = salesOrder.SalesOrderDetails.OrderBy(e => e.CreatedOn).ToList();
            }
            return salesOrder;
        }
        public override IEnumerable<SalesOrderHeader> Find(Expression<Func<SalesOrderHeader, bool>> predicate)
        {
            return dbSet
                .AsNoTracking()                
                .Include("SalesOrderDetails.Reference")
                .Include(b => b.Transports)
                .Include(b => b.ExternalServices).ThenInclude(es => es.Details)
                .Where(predicate)
                .OrderBy(s => s.Number);
        }

        public SalesOrderDetail? GetDetailById(Guid id)
        {
            var salesOrderDetail = _salesOrderDetailRepository.Find(c => c.Id == id).FirstOrDefault();
            return salesOrderDetail;
        }
        
        public async Task AddDetail(SalesOrderDetail detail)
        {
            await _salesOrderDetailRepository.Add(detail);
        }
        public async Task UpdateDetail(SalesOrderDetail detail)
        {
            await _salesOrderDetailRepository.Update(detail);
        }
        public async Task<bool> RemoveDetail(SalesOrderDetail detail)
        {   
            await _salesOrderDetailRepository.Remove(detail);
            return true;            
            
        }
    }
}
