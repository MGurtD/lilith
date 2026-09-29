using Domain.Entities.Warehouse;

namespace Application.Contracts
{
    public interface IWarehouseRepository : IRepository<Domain.Entities.Warehouse.Warehouse, Guid>
    {
        IRepository<Location, Guid> Locations { get; }

        Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetBySiteId(Guid siteId);

        Task<IEnumerable<Domain.Entities.Warehouse.Warehouse>> GetAllWithLocations();

        Task<Location?> GetDefaultLocation();

        Task<IEnumerable<StockListItemResponse>> GetStockList(Guid? locationId, Guid? referenceId);

        Task<IEnumerable<StockResponse>> GetStockByReferenceId(Guid referenceId);

        /// <summary>
        /// True when the location holds stock, has stock movements or is the
        /// default location of a warehouse.
        /// </summary>
        Task<bool> IsLocationInUse(Guid locationId);

        /// <summary>
        /// True when any location of the warehouse is in use (see <see cref="IsLocationInUse"/>).
        /// </summary>
        Task<bool> IsInUse(Guid warehouseId);
    }
}
