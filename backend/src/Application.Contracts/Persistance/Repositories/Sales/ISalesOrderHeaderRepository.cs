using Domain.Entities.Sales;

namespace Application.Contracts
{
    public interface ISalesOrderHeaderRepository : IRepository<SalesOrderHeader, Guid>
    {
        IRepository<SalesOrderTransport, Guid> Transports { get; }  
        IRepository<SalesOrderExternalServices, Guid> ExternalServices { get; }
        IRepository<SalesOrderExternalServiceDetail, Guid> ExternalServiceDetails { get; }
        IRepository<SalesOrderDetailPhaseProfit, Guid> DetailPhaseProfits { get; }

        SalesOrderDetail? GetDetailById(Guid id);
        Task AddDetail(SalesOrderDetail detail);
        Task UpdateDetail(SalesOrderDetail detail);
        Task<bool> RemoveDetail(SalesOrderDetail detail);
    }
}

