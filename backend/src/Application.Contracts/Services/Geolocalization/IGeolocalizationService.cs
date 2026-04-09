namespace Application.Contracts.Services.Geolocalization;

public class Coordinates
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

public interface IGeolocalizationService
{
    Task<Coordinates?> GetCoordinatesAsync(string address, string city, string postalCode, string country);
    Task<decimal?> GetDistanceAsync(Coordinates origin, Coordinates destination);
}
