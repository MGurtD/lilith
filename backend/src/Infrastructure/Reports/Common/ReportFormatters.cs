using System.Globalization;

namespace Infrastructure.Reports.Common;

public static class ReportFormatters
{
    public static CultureInfo Culture(string languageCode) => CultureInfo.GetCultureInfo(string.IsNullOrWhiteSpace(languageCode) ? "ca" : languageCode);
    public static string Date(DateTime value, CultureInfo culture) => value.ToString("d", culture);
    public static string Quantity(decimal value, CultureInfo culture) => value.ToString("N0", culture);
    public static string Amount(decimal value, CultureInfo culture) => value.ToString("N2", culture);
    public static string Currency(decimal value, CultureInfo culture) => $"{Amount(value, culture)} €";
    public static string Locality(string postalCode, string city, string region)
    {
        var locality = string.Join(" – ", new[] { postalCode, city }.Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(region) ? locality : $"{locality} ({region})";
    }
}