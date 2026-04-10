using System.ComponentModel.DataAnnotations;

namespace Application.Contracts;

public class CreateApiKeyRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Scopes { get; set; } = string.Empty;

    public DateTime? ExpiresOn { get; set; }
}
