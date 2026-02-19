namespace Domain.Constants;

public static class LocationTypeConstants
{
    public const string Supply    = "Supply";
    public const string Receiving = "Receiving";
    public const string Shipping  = "Shipping";
    public const string Storage   = "Storage";

    public static readonly IReadOnlyList<string> All =
    [
        Supply,
        Receiving,
        Shipping,
        Storage
    ];

    public static bool IsValid(string value) =>
        All.Contains(value, StringComparer.OrdinalIgnoreCase);
}
