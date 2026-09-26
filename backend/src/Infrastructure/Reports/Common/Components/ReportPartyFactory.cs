using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;

namespace Infrastructure.Reports.Common.Components;

public static class ReportPartyFactory
{
    public static ReportParty Site(Site site, Enterprise enterprise) => new(enterprise.Name, [site.Address, ReportFormatters.Locality(site.PostalCode, site.City, site.Region), site.PhoneNumber, string.IsNullOrWhiteSpace(site.EmailSales) ? site.Email : site.EmailSales], site.VatNumber);
    public static ReportParty Customer(Customer customer)
    {
        var address = customer.MainAddress();
        return new(customer.TaxName, [customer.ComercialName, address?.Address ?? string.Empty, address is null ? string.Empty : ReportFormatters.Locality(address.PostalCode, address.City, address.Region)], customer.VatNumber);
    }
    public static ReportParty Supplier(Supplier supplier) => new(supplier.TaxName, [supplier.ComercialName, supplier.Address, ReportFormatters.Locality(supplier.PostalCode, supplier.City, supplier.Region), supplier.Phone], supplier.VatNumber);
}