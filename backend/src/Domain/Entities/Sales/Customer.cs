namespace Domain.Entities.Sales
{
    public class Customer : Entity
    {
        public string Code { get; set; } = string.Empty;
        public string ComercialName { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
        public string Web { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Observations { get; set; } = string.Empty;
        public string InvoiceNotes { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = "ca";

        public bool IsValidForSales()
        {
            var invalid = string.IsNullOrWhiteSpace(TaxName) || string.IsNullOrWhiteSpace(VatNumber) || string.IsNullOrWhiteSpace(AccountNumber);
            return !invalid;
        }

        public CustomerAddress? MainAddress()
        {
            var enabledAddresses = Address.Where(c => !c.Disabled);

            if (enabledAddresses.Any())
            {
                if (enabledAddresses.Any(c => c.Main)) return enabledAddresses.FirstOrDefault(c => c.Main);
                return enabledAddresses.FirstOrDefault();
            }
            return null;
        }

        public CustomerContact? MainContact()
        {
            if (Contacts.Any())
            {
                if (Contacts.Any(c => c.Main)) return Contacts.FirstOrDefault(c => c.Main);
                return Contacts.FirstOrDefault();
            }
            return null;
        }

        public PaymentMethod? PaymentMethod { get; set; }
        public Guid? PaymentMethodId { get; set; }

        public CustomerType? Type { get; set; }
        public Guid CustomerTypeId { get; set; }


        public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
        public ICollection<CustomerAddress> Address { get; set; } = new List<CustomerAddress>();
    }
}
