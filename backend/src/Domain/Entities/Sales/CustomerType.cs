namespace Domain.Entities.Sales
{
    public class CustomerType : Entity, IMasterData
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
