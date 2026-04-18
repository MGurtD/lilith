namespace Application.Contracts.Services.Geolocalization;

public class Coordinates
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    /// <summary>
    /// Calcula la distancia en línea recta (km) entre dos coordenadas usando la fórmula de Haversine.
    /// Se usa como fallback silencioso cuando la API de rutas no está disponible.
    /// </summary>
    public static decimal HaversineDistanceKm(Coordinates origin, Coordinates destination)
    {
        const double earthRadiusKm = 6371.0;

        var lat1 = (double)origin.Latitude * Math.PI / 180.0;
        var lat2 = (double)destination.Latitude * Math.PI / 180.0;
        var dLat = ((double)(destination.Latitude - origin.Latitude)) * Math.PI / 180.0;
        var dLon = ((double)(destination.Longitude - origin.Longitude)) * Math.PI / 180.0;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return Math.Round((decimal)(earthRadiusKm * c), 2);
    }
}

public interface IGeolocalizationService
{
    Task<Coordinates?> GetCoordinatesAsync(string address, string city, string postalCode, string country);
    Task<decimal?> GetDistanceAsync(Coordinates origin, Coordinates destination);
}
