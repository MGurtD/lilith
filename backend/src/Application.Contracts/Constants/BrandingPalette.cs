namespace Application.Contracts;

public static class BrandingPalette
{
    public const string Black = "black";
    public const string Blue = "blue";
    public const string Indigo = "indigo";
    public const string Emerald = "emerald";
    public const string Teal = "teal";
    public const string Violet = "violet";
    public const string Orange = "orange";
    public const string Rose = "rose";
    public const string Default = Blue;

    public static readonly IReadOnlyList<string> All =
    [
        Black,
        Blue,
        Indigo,
        Emerald,
        Teal,
        Violet,
        Orange,
        Rose
    ];

    public static bool IsAllowed(string? value) =>
        value is not null && All.Contains(value, StringComparer.Ordinal);

    public static bool TryNormalize(string? value, out string? normalized)
    {
        normalized = value?.Trim().ToLowerInvariant();
        return value is null || IsAllowed(normalized);
    }

    public static string NormalizeOrDefault(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return IsAllowed(normalized) ? normalized! : Default;
    }
}
