using Domain.Entities.Warehouse;

namespace Application.Contracts
{
    public interface IWarehouseRepository : IRepository<Domain.Entities.Warehouse.Warehouse, Guid>
    {
        IRepository<Location, Guid> Locations { get; }

        Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetBySiteId(Guid siteId);

        Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetAllWithLocations();

        Task<Location?> GetDefaultLocation();
    }
}
