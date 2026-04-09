namespace Domain.Entities.Purchase
{
    public class Supplier : Entity
    {
        public string ComercialName { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Observations { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public SupplierType? Type { get; set; }
        public Guid SupplierTypeId { get; set; }
        public decimal DistanceFromSite { get; set; } = 0;
        public decimal Latitude { get; set; } = 0;
        public decimal Longitude { get; set; } = 0;

        public PaymentMethod? PaymentMethod { get; set; }
        public Guid? PaymentMethodId { get; set; }

        public ICollection<SupplierContact> Contacts { get; } = new List<SupplierContact>();
    }
}
