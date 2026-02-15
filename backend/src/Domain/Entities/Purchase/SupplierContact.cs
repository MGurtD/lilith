namespace Domain.Entities.Purchase
{
    public class SupplierContact : Entity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Charge { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PhoneExtension { get; set; } = string.Empty;
        public string Observations { get; set; } = string.Empty;
        public bool Default { get; set; } = false;

        public Guid SupplierId { get; set; }
    }
}
