namespace Domain.Entities.Sales
{
    public class CustomerContact : Entity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Charge { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool Main { get; set; } = false;

        public Guid CustomerId { get; set; }
        public Guid? CustomerAddressId { get; set; }
    }
}