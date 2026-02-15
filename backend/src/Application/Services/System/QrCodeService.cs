using System.Text;
using Application.Contracts;
using QRCoder;

namespace Application.Services.System;

public class QrCodeService : IQrCodeService
{
    public string GenerateSvgBase64(string content, int pixelsPerModule = 5)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("QR content cannot be null or empty.", nameof(content));

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

        var svgQrCode = new SvgQRCode(qrCodeData);
        string svg = svgQrCode.GetGraphic(pixelsPerModule);
        byte[] svgBytes = Encoding.UTF8.GetBytes(svg);

        return $"data:image/svg+xml;base64,{Convert.ToBase64String(svgBytes)}";
    }

    public string GeneratePngBase64(string content, int pixelsPerModule = 20)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("QR content cannot be null or empty.", nameof(content));

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

        var pngQrCode = new PngByteQRCode(qrCodeData);
        byte[] pngBytes = pngQrCode.GetGraphic(pixelsPerModule);

        return $"data:image/png;base64,{Convert.ToBase64String(pngBytes)}";
    }
}
