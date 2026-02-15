namespace Domain.Entities.Sales
{
    public class CustomerAddress : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool Main { get; set; } = false;
        public string Observations { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
    }
}
