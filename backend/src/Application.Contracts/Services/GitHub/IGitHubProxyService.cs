using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Services.GitHub;

public class CreateDraftIssueRequest
{
    [Required]
    public string Resum { get; set; } = string.Empty;
    public string Descripcio { get; set; } = string.Empty;
}

public class CreateDraftIssueResult
{
    public string Id { get; set; } = string.Empty;
}

public interface IGitHubProxyService
{
    Task<GenericResponse> CreateDraftIssue(CreateDraftIssueRequest request);
}
