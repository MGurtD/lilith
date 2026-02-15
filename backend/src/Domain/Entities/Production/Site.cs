namespace Domain.Entities.Production
{
    public class Site : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmailSales { get; set; } = string.Empty;
        public string EmailPurchase { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;

        public bool IsValidForSales()
        {
            var invalid = string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(City) ||
                          string.IsNullOrWhiteSpace(PostalCode) || string.IsNullOrWhiteSpace(Region) || 
                          string.IsNullOrWhiteSpace(Country) || string.IsNullOrWhiteSpace(VatNumber);
            return !invalid;
        }

        public Guid EnterpriseId { get; set; }
        public Enterprise? Enterprise { get; set; }
    }
}
