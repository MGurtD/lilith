namespace Verifactu.Contracts;

public class VerifactuResponse
{
    public bool Success { get; set; }
    public string StatusRegister { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string XmlRequest { get; set; } = string.Empty;
    public string XmlResponse { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string QrCodeUrl { get; set; } = string.Empty;
}
