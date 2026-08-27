using Domain.Entities.Sales;

namespace Application.Tests.TestData;

/// <summary>Builder for <see cref="Customer"/> test data.</summary>
public static class CustomerBuilder
{
    private const string DefaultVat = "12345678Z";

    /// <summary>
    /// Returns a Customer that passes all fiscal validation in CustomerService:
    /// valid NIF, non-empty TaxName/AccountNumber, and one complete Main address.
    /// </summary>
    public static Customer Valid(string? vatNumber = null)
    {
        var id = Guid.NewGuid();
        return new Customer
        {
            Id = id,
            Code = "C001",
            ComercialName = "Acme SA",
            TaxName = "Acme Sociedad Anónima",
            VatNumber = vatNumber ?? DefaultVat,
            AccountNumber = "ES7621000000000000000000",
            Address = new List<CustomerAddress>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = id,
                    Main = true,
                    Country = "Espanya",
                    PostalCode = "08001",
                    City = "Barcelona",
                    Address = "C/ Major 1",
                },
            },
        };
    }

    /// <summary>Returns a valid customer address that is missing the <paramref name="missingField"/>.</summary>
    public static CustomerAddress IncompleteAddress(Guid customerId, string missingField) =>
        new()
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Main = true,
            Country = missingField == "Country" ? "" : "Espanya",
            PostalCode = missingField == "PostalCode" ? "" : "08001",
            City = missingField == "City" ? "" : "Barcelona",
            Address = missingField == "Address" ? "" : "C/ Major 1",
        };
}
