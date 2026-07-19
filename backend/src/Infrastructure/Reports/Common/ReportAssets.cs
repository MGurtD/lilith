namespace Infrastructure.Reports.Common;

public static class ReportAssets
{
    private static readonly Lazy<byte[]> CompanyLogo = new(() => LoadResource("temges-logo.jpg"));
    private static readonly Lazy<byte[]> WatermarkLogo = new(() => LoadResource("temges-watermark.png"));

    public static byte[] Logo => CompanyLogo.Value;
    public static byte[] Watermark => WatermarkLogo.Value;

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