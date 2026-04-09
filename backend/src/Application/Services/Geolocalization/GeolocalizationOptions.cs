namespace Application.Services.Geolocalization;

public class GeolocalizationOptions
{
    public const string SectionName = "Geolocalization";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openrouteservice.org";
}
