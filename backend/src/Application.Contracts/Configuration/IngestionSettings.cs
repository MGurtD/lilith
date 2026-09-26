namespace Application.Contracts;

public class IngestionSettings
{
    public string ApiKey { get; set; } = string.Empty;
    /// <summary>LlamaCloud EU region. API keys are region-specific.</summary>
    public string BaseUrl { get; set; } = "https://api.cloud.eu.llamaindex.ai";
    public string ProjectId { get; set; } = string.Empty;
    public string Tier { get; set; } = "agentic";
    public string Version { get; set; } = "2026-03-31";
    public bool ConfidenceScores { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 90;
    /// <summary>Fields whose provider confidence is below this value are flagged for review.</summary>
    public decimal LowConfidenceThreshold { get; set; } = 0.8m;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("Ingestion:ApiKey configuration key is required");
        if (string.IsNullOrWhiteSpace(ProjectId))
            throw new ArgumentException("Ingestion:ProjectId configuration key is required");
        if (TimeoutSeconds <= 0)
            throw new ArgumentException("Ingestion:TimeoutSeconds must be greater than zero");
        if (LowConfidenceThreshold is < 0 or > 1)
            throw new ArgumentException("Ingestion:LowConfidenceThreshold must be between 0 and 1");
    }
}
