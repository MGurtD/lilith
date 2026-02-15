namespace Domain.Entities
{
    public class Tax : Entity
    {
        public string Name { get; set; } = string.Empty;
        public decimal Percentatge { get; set; }

        public decimal ApplyTax(decimal amount)
        {
            return amount / 100 * Percentatge;
        }
    }
}
