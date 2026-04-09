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
        public decimal DistanceFromSite { get; set; } = 0;
        public decimal Latitude { get; set; } = 0;
        public decimal Longitude { get; set; } = 0;
        public string Observations { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
    }
}
