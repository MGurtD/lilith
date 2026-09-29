using QuestPDF.Drawing;

namespace Infrastructure.Reports.Common;

/// <summary>
/// Registers IBM Plex Sans (SIL OFL 1.1), the typeface of the UI, so reports render the same on every host.
/// </summary>
public static class ReportFonts
{
    public const string Family = "IBM Plex Sans";

    private static readonly string[] Files =
    [
        "IBMPlexSans-Regular.ttf",
        "IBMPlexSans-Italic.ttf",
        "IBMPlexSans-SemiBold.ttf",
        "IBMPlexSans-Bold.ttf",
    ];

    private static readonly Lazy<bool> Registration = new(Register);

    public static void EnsureRegistered() => _ = Registration.Value;

    private static bool Register()
    {
        var assembly = typeof(ReportFonts).Assembly;
        foreach (var file in Files)
        {
            var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(name => name.EndsWith($".{file}", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Embedded report font was not found: {file}");
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded report font cannot be read: {file}");
            FontManager.RegisterFontWithCustomName(Family, stream);
        }

        return true;
    }
}
