using Application.Contracts;
using Application.Contracts.Services.Geolocalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Services.Geolocalization;

public class GeolocalizationService(
    HttpClient httpClient, 
    IOptions<AppSettings> options,
    ILogger<GeolocalizationService> logger) : IGeolocalizationService
{
    private readonly GeolocalizationSettings? _options = options.Value.Geolocalization;

    public async Task<Coordinates?> GetCoordinatesAsync(string address, string city, string postalCode, string country)
    {
        logger.LogInformation("GetCoordinatesAsync Input -> Address: {Address}, City: {City}, PostalCode: {PostalCode}, Country: {Country}", address, city, postalCode, country);

        if (string.IsNullOrWhiteSpace(_options?.ApiKey))
        {
            logger.LogWarning("Geolocalization API Key is missing.");
            return null;
        }

        var fullAddress = $"{address}, {postalCode} {city}, {country}";
        
        try
        {
            // OpenRouteService geocoding endpoint
            var url = $"{_options?.BaseUrl}/geocode/search?api_key={_options?.ApiKey}&text={Uri.EscapeDataString(fullAddress)}";
            var response = await httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Error getting coordinates for {Address}. Status: {Status}", fullAddress, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            logger.LogInformation("GetCoordinatesAsync Response -> JSON: {Json}", json.GetRawText());
            
            if (json.TryGetProperty("features", out var features) && features.GetArrayLength() > 0)
            {
                var firstFeature = features[0];
                if (firstFeature.TryGetProperty("geometry", out var geometry))
                {
                    if (geometry.TryGetProperty("coordinates", out var coordinatesArray) && coordinatesArray.GetArrayLength() >= 2)
                    {
                        var lon = coordinatesArray[0].GetDecimal();
                        var lat = coordinatesArray[1].GetDecimal();
                        logger.LogInformation("GetCoordinatesAsync Output -> Latitude: {Lat}, Longitude: {Lon}", lat, lon);
                        return new Coordinates { Latitude = lat, Longitude = lon };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while getting coordinates for {Address}", fullAddress);
        }

        return null;
    }

    public async Task<decimal?> GetDistanceAsync(Coordinates origin, Coordinates destination)
    {
        logger.LogInformation("GetDistanceAsync Input -> Origin: [{OrgLat}, {OrgLon}] | Destination: [{DstLat}, {DstLon}]", origin.Latitude, origin.Longitude, destination.Latitude, destination.Longitude);

        if (string.IsNullOrWhiteSpace(_options?.ApiKey))
        {
            return null;
        }

        try
        {
            // OpenRouteService directions endpoint
            var url = $"{_options?.BaseUrl}/v2/directions/driving-car/geojson";
            
            // Note: ORS expects [longitude, latitude]
            var requestBody = new
            {
                coordinates = new[]
                {
                    new[] { origin.Longitude, origin.Latitude },
                    new[] { destination.Longitude, destination.Latitude }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Bearer {_options!.ApiKey}");
            request.Content = JsonContent.Create(requestBody);

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogError("Error calculating distance. Status: {Status}, Body: {Error}", response.StatusCode, error);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            logger.LogInformation("GetDistanceAsync Response -> JSON: {Json}", json.GetRawText());
            
            if (json.TryGetProperty("features", out var features) && features.GetArrayLength() > 0)
            {
                var firstFeature = features[0];
                if (firstFeature.TryGetProperty("properties", out var properties))
                {
                    if (properties.TryGetProperty("summary", out var summary))
                    {
                        if (summary.TryGetProperty("distance", out var distanceMeters))
                        {
                            // distance comes in meters, we return km
                            var distanceKm = Math.Round(distanceMeters.GetDecimal() / 1000m, 2);
                            logger.LogInformation("GetDistanceAsync Output -> Distance: {DistanceKm} km", distanceKm);
                            return distanceKm;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while calculating distance");
        }

        return null;
    }
}
