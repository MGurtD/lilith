using Domain.Entities.Production;
using Domain.Entities.Shared;

namespace Domain.Entities.Sales
{
    public class SalesInvoice : Entity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TransportAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal NetAmount { get; set; }

        public Exercise? Exercise { get; set; }
        public Guid? ExerciseId { get; set; }
        public InvoiceSerie? InvoiceSerie { get; set; }
        public Guid? InvoiceSerieId { get; set; }
        public Guid? StatusId { get; set; }
        public Status? Status { get; set; }
        public Guid? IntegrationStatusId { get; set; }
        public Status? IntegrationStatus { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public Guid PaymentMethodId { get; set; }
        public SalesInvoice? ParentSalesInvoice { get; set; }
        public Guid? ParentSalesInvoiceId { get; set; }

        public Customer? Customer { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerComercialName { get; set; } = string.Empty;
        public string CustomerTaxName { get; set; } = string.Empty; 
        public string CustomerVatNumber { get; set; } = string.Empty;
        public string CustomerAccountNumber { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string CustomerPostalCode { get; set; } = string.Empty;
        public string CustomerRegion { get; set; } = string.Empty;
        public string CustomerCountry { get; set; } = string.Empty;              

        public Site? Site { get; set; }
        public Guid? SiteId { get; set;}
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;

        public ICollection<SalesInvoiceDetail> SalesInvoiceDetails { get; set; } = [];
        public ICollection<SalesInvoiceDueDate> SalesInvoiceDueDates { get; set; } = [];
        public ICollection<SalesInvoiceImport> SalesInvoiceImports { get; set; } = [];
        public ICollection<SalesInvoiceVerifactuRequest> VerifactuRequests { get; set; } = [];

        public void SetCustomer(Customer customer)
        {
            CustomerId = customer.Id;
            PaymentMethodId = customer.PaymentMethodId!.Value;
            CustomerAccountNumber = customer.AccountNumber;
            CustomerComercialName = customer.ComercialName;
            CustomerTaxName = customer.TaxName;
            CustomerVatNumber = customer.VatNumber;

            if (customer.MainAddress() != null)
            {
                var mainAddress = customer.MainAddress()!;
                CustomerRegion = mainAddress.Region;
                CustomerCountry = mainAddress.Country;
                CustomerCity = mainAddress.City;
                CustomerPostalCode = mainAddress.PostalCode;
                CustomerAddress = mainAddress.Address;
            }
        }

        public void SetSite(Site site)
        {
            SiteId = site.Id;
            Name = site.Name;
            Address = site.Address;
            City = site.City;
            PostalCode = site.PostalCode;
            Region = site.Region;
            Country = site.Country;
            VatNumber = site.VatNumber;
        }

        public void CalculateAmountsFromImports()
        {
            BaseAmount = 0;
            TaxAmount = 0;
            GrossAmount = 0;
            NetAmount = 0;

            if (SalesInvoiceImports == null || SalesInvoiceImports.Count == 0) return;

            foreach(var import in SalesInvoiceImports) 
            {
                BaseAmount += import.BaseAmount;
                TaxAmount += import.TaxAmount;
            }
            GrossAmount = BaseAmount + TransportAmount + TaxAmount;
            NetAmount = GrossAmount; // TODO > Aplicar descomptes
        }

    }
}
