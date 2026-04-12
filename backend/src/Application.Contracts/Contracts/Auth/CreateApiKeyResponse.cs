namespace Application.Contracts;

public class CreateApiKeyResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public DateTime? ExpiresOn { get; set; }
}
