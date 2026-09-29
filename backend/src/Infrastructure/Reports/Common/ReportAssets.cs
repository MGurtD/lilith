namespace Infrastructure.Reports.Common;

/// <summary>
/// Bundled fallbacks used when the tenant has not uploaded its own logo or watermark.
/// </summary>
public static class ReportAssets
{
    private static readonly Lazy<byte[]> DefaultLogo = new(() => LoadResource("temges-logo.jpg"));
    private static readonly Lazy<byte[]> DefaultWatermark = new(() => LoadResource("temges-watermark.png"));

    public static byte[] Logo => DefaultLogo.Value;
    public static byte[] Watermark => DefaultWatermark.Value;

    private static byte[] LoadResource(string resourceSuffix)
    {
        var assembly = typeof(ReportAssets).Assembly;
        var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(name => name.EndsWith($".{resourceSuffix}", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Embedded report resource was not found.");
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Embedded report resource cannot be read.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
