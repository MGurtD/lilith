namespace Application.Contracts;

public class AppSettings
{
    public ConnectionStringsSettings ConnectionStrings { get; set; } = new();
    public JwtConfigSettings JwtConfig { get; set; } = new();
    public FileManagmentSettings FileManagment { get; set; } = new();
    public VerifactuSettings? Verifactu { get; set; }
    public void Validate()
    {
        ConnectionStrings.Validate();
        JwtConfig.Validate();
        FileManagment.Validate();
        if (Verifactu != null)
            Verifactu?.Validate();
    }
}

public class ConnectionStringsSettings
{
    public string Default { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Default))
            throw new ArgumentException("ConnectionStrings:Default configuration key is required");
    }
}

public class JwtConfigSettings
{
    public string Secret { get; set; } = string.Empty;
    public string ExpirationTimeFrame { get; set; } = "00:05:00";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Secret))
            throw new ArgumentException("JwtConfig:Secret configuration key is required");
        if (string.IsNullOrWhiteSpace(ExpirationTimeFrame))
            throw new ArgumentException("JwtConfig:ExpirationTimeFrame configuration key is required");
        if (!TimeSpan.TryParse(ExpirationTimeFrame, out _))
            throw new ArgumentException("JwtConfig:ExpirationTimeFrame must be a valid TimeSpan");
    }

    public TimeSpan ExpirationTime
    {
        get
        {
            if (TimeSpan.TryParse(ExpirationTimeFrame, out var ts))
                return ts;
            throw new ArgumentException("JwtConfig:ExpirationTimeFrame must be a valid TimeSpan");
        }
    }
}

public class FileManagmentSettings
{
    public string UploadPath { get; set; } = string.Empty;
    public long LimitSize { get; set; } = 200;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(UploadPath))
            throw new ArgumentException("FileManagment:UploadPath configuration key is required");
        if (LimitSize <= 0)
            throw new ArgumentException("FileManagment:LimitSize must be greater than zero");
    }
}

public class VerifactuSettings
{
    public string Url { get; set; } = string.Empty;
    public string UrlQr { get; set; } = string.Empty;
    public CertificateSettings Certificate { get; set; } = new();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Url))
            throw new ArgumentException("Verifactu:Url configuration key is required");
        if (string.IsNullOrWhiteSpace(UrlQr))
            throw new ArgumentException("Verifactu:UrlQr configuration key is required");
        Certificate.Validate();
    }
}

public class CertificateSettings
{
    public string Path { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new ArgumentException("Verifactu:Certificate:Path configuration key is required");
        if (string.IsNullOrWhiteSpace(Password))
            throw new ArgumentException("Verifactu:Certificate:Password configuration key is required");
    }
}
