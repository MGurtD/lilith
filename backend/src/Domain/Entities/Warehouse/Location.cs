namespace Domain.Entities.Warehouse
{
    public class Location : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public string? LocationType { get; set; }
    }
}