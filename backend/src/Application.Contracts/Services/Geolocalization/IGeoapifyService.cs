namespace Application.Contracts.Services.Geolocalization;

public class AddressAutocompleteRequest
{
    public string Text { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public int? Limit { get; set; }
    public string? Type { get; set; }
}

public class AddressAutocompleteResult
{
    public string PlaceId { get; set; } = string.Empty;
    public string Formatted { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public decimal Lat { get; set; }
    public decimal Lon { get; set; }
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Postcode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Housenumber { get; set; } = string.Empty;
    public string ResultType { get; set; } = string.Empty;
}

public interface IGeoapifyService
{
    Task<List<AddressAutocompleteResult>> AutocompleteAsync(AddressAutocompleteRequest request);
}
