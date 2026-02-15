using Domain.Entities.Sales;

namespace Application.Contracts
{
    public interface ISalesOrderHeaderRepository : IRepository<SalesOrderHeader, Guid>
    {
        SalesOrderDetail? GetDetailById(Guid id);
        Task AddDetail(SalesOrderDetail detail);
        Task UpdateDetail(SalesOrderDetail detail);
        Task<bool> RemoveDetail(SalesOrderDetail detail);
    }
}

