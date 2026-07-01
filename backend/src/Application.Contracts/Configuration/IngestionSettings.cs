namespace Application.Contracts;

public class IngestionSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.cloud.llamaindex.ai";
    public string ProjectId { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "llama-parse";
    public string Tier { get; set; } = "agentic";
    public string Version { get; set; } = "2026-03-31";
    public bool ConfidenceScores { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 90;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("Ingestion:ApiKey configuration key is required");
        if (string.IsNullOrWhiteSpace(ProjectId))
            throw new ArgumentException("Ingestion:ProjectId configuration key is required");
        if (TimeoutSeconds <= 0)
            throw new ArgumentException("Ingestion:TimeoutSeconds must be greater than zero");
    }
}