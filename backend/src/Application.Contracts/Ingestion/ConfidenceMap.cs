namespace Application.Contracts.Ingestion;

public class ConfidenceMap
{
    public Dictionary<string, decimal> Headers { get; set; } = new();
    public List<decimal> Lines { get; set; } = new();
}