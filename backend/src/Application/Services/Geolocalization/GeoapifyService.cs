using Application.Contracts;
using Application.Contracts.Services.Geolocalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Services.Geolocalization;

public class GeoapifyService(
    HttpClient httpClient,
    IOptions<AppSettings> options,
    ILogger<GeoapifyService> logger) : IGeoapifyService
{
    private readonly GeoapifySettings? _settings = options.Value.Geoapify;

    public async Task<List<AddressAutocompleteResult>> AutocompleteAsync(AddressAutocompleteRequest request)
    {
        if (string.IsNullOrWhiteSpace(_settings?.ApiKey))
        {
            logger.LogWarning("Geoapify API Key is missing.");
            return [];
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return [];
        }

        var limit = request.Limit ?? _settings.DefaultLimit;

        try
        {
            var queryParams = $"text={Uri.EscapeDataString(request.Text)}" +
                              $"&format=json" +
                              $"&limit={limit}" +
                              $"&apiKey={_settings.ApiKey}";

            if (!string.IsNullOrWhiteSpace(request.CountryCode))
            {
                queryParams += $"&lang={Uri.EscapeDataString(request.CountryCode)}";
                queryParams += $"&filter=countrycode:{Uri.EscapeDataString(request.CountryCode)}";
            }

            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                queryParams += $"&type={Uri.EscapeDataString(request.Type)}";
            }

            var url = $"{_settings.BaseUrl}/v1/geocode/autocomplete?{queryParams}";

            logger.LogInformation("Geoapify Autocomplete request: text={Text}, countryCode={CountryCode}, limit={Limit}",
                request.Text, request.CountryCode, limit);

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Geoapify Autocomplete error. Status: {Status}", response.StatusCode);
                return [];
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (!json.TryGetProperty("results", out var results))
            {
                return [];
            }

            var items = new List<AddressAutocompleteResult>();

            foreach (var item in results.EnumerateArray())
            {
                items.Add(new AddressAutocompleteResult
                {
                    PlaceId = GetStringProperty(item, "place_id"),
                    Formatted = GetStringProperty(item, "formatted"),
                    AddressLine1 = GetStringProperty(item, "address_line1"),
                    AddressLine2 = GetStringProperty(item, "address_line2"),
                    Lat = GetDecimalProperty(item, "lat"),
                    Lon = GetDecimalProperty(item, "lon"),
                    Country = GetStringProperty(item, "country"),
                    CountryCode = GetStringProperty(item, "country_code"),
                    State = GetStringProperty(item, "state"),
                    City = GetCityProperty(item),
                    Postcode = GetStringProperty(item, "postcode"),
                    Street = GetStringProperty(item, "street"),
                    Housenumber = GetStringProperty(item, "housenumber"),
                    ResultType = GetStringProperty(item, "result_type"),
                });
            }

            logger.LogInformation("Geoapify Autocomplete returned {Count} results for text={Text}", items.Count, request.Text);
            return items;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during Geoapify Autocomplete for text={Text}", request.Text);
            return [];
        }
    }

    private static string GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString() ?? string.Empty
            : string.Empty;
    }

    private static decimal GetDecimalProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number
            ? prop.GetDecimal()
            : 0m;
    }

    /// <summary>
    /// Geoapify returns city in different properties depending on the location type.
    /// Falls back through city → town → village → municipality.
    /// </summary>
    private static string GetCityProperty(JsonElement element)
    {
        string[] cityFields = ["city", "town", "village", "municipality"];
        foreach (var field in cityFields)
        {
            var value = GetStringProperty(element, field);
            if (!string.IsNullOrEmpty(value))
                return value;
        }
        return string.Empty;
    }
}
