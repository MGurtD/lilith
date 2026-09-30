namespace Domain.Entities.Purchase
{
    public class SupplierType : Entity, IMasterData
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
