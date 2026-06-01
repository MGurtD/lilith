namespace Domain.Entities
{
    public class Tax : Entity
    {
        public string Name { get; set; } = string.Empty;
        public decimal Percentatge { get; set; }
        public bool IsReverseCharge { get; set; }

        public decimal ApplyTax(decimal amount)
        {
            if (IsReverseCharge) return 0m;
            return amount / 100 * Percentatge;
        }
    }
}
