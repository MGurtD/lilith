using Application.Contracts;
using Application.Contracts.Services.GitHub;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Services.GitHub;

public class GitHubProxyService(
    HttpClient httpClient,
    IOptions<AppSettings> options,
    ILocalizationService localizationService,
    ILogger<GitHubProxyService> logger) : IGitHubProxyService
{
    private readonly GitHubSettings? _settings = options.Value.GitHub;

    private const string CreateDraftIssueMutation = @"mutation CreateDraftIssue($projectId: ID!, $title: String!, $body: String!) {
  addProjectV2DraftIssue(input: { projectId: $projectId, title: $title, body: $body }) {
    projectItem { id }
  }
}";

    public async Task<GenericResponse> CreateDraftIssue(CreateDraftIssueRequest request)
    {
        if (_settings == null || string.IsNullOrWhiteSpace(_settings.Token))
        {
            logger.LogWarning("GitHub integration is not configured. Token is missing.");
            return new GenericResponse(false, localizationService.GetLocalizedString("GitHubNotConfigured"));
        }

        try
        {
            var payload = new
            {
                query = CreateDraftIssueMutation,
                variables = new
                {
                    projectId = _settings.ProjectId,
                    title = request.Resum,
                    body = request.Descripcio
                }
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.GraphQlUrl)
            {
                Content = JsonContent.Create(payload)
            };
            httpRequest.Headers.Add("Authorization", $"Bearer {_settings.Token}");
            httpRequest.Headers.Add("User-Agent", _settings.UserAgent);

            var response = await httpClient.SendAsync(httpRequest);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            // GitHub may return errors with an HTTP 200 status code.
            if (json.TryGetProperty("errors", out var errors) &&
                errors.ValueKind == JsonValueKind.Array &&
                errors.GetArrayLength() > 0)
            {
                var messages = new List<string>();
                foreach (var error in errors.EnumerateArray())
                {
                    if (error.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                        messages.Add(message.GetString() ?? string.Empty);
                }

                logger.LogError("GitHub CreateDraftIssue returned errors: {Errors}", string.Join(" | ", messages));

                if (messages.Count == 0)
                    messages.Add(localizationService.GetLocalizedString("GitHubDraftIssueError"));

                return new GenericResponse(false, messages);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("GitHub CreateDraftIssue failed. Status: {Status}", response.StatusCode);
                return new GenericResponse(false, localizationService.GetLocalizedString("GitHubDraftIssueError"));
            }

            if (json.TryGetProperty("data", out var data) &&
                data.TryGetProperty("addProjectV2DraftIssue", out var draftIssue) &&
                draftIssue.TryGetProperty("projectItem", out var projectItem) &&
                projectItem.TryGetProperty("id", out var id) &&
                id.ValueKind == JsonValueKind.String)
            {
                var itemId = id.GetString() ?? string.Empty;
                logger.LogInformation("GitHub CreateDraftIssue created project item {ItemId}", itemId);
                return new GenericResponse(true, new CreateDraftIssueResult { Id = itemId });
            }

            logger.LogError("GitHub CreateDraftIssue response did not contain the expected project item id.");
            return new GenericResponse(false, localizationService.GetLocalizedString("GitHubDraftIssueError"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while creating GitHub draft issue for title={Title}", request.Resum);
            return new GenericResponse(false, localizationService.GetLocalizedString("GitHubDraftIssueError"));
        }
    }
}
