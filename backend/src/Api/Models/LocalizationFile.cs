using System.Text.Json.Serialization;

namespace Api.Models
{
    public class LocalizationFile
    {
        [JsonPropertyName("culture")]
        public string Culture { get; set; } = string.Empty;
        
        [JsonPropertyName("texts")]
        public Dictionary<string, string> Texts { get; set; } = new();
    }
}