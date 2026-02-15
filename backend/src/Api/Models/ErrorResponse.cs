using System.Text.Json.Serialization;

namespace Api.Models
{
    /// <summary>
    /// RFC 7807 Problem Details response model for standardized error responses.
    /// Maintains compatibility with existing GenericResponse structure.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// A URI reference that identifies the problem type (e.g., "https://httpstatuses.com/500")
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// A short, human-readable summary of the problem type (localized)
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The HTTP status code
        /// </summary>
        [JsonPropertyName("status")]
        public int Status { get; set; }

        /// <summary>
        /// A human-readable explanation specific to this occurrence (localized)
        /// </summary>
        [JsonPropertyName("detail")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Detail { get; set; }

        /// <summary>
        /// A URI reference that identifies the specific occurrence (request path)
        /// </summary>
        [JsonPropertyName("instance")]
        public string Instance { get; set; } = string.Empty;

        /// <summary>
        /// Correlation/Trace ID for request tracking and support tickets
        /// </summary>
        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Collection of error messages (maintains GenericResponse compatibility)
        /// </summary>
        [JsonPropertyName("errors")]
        public IList<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}
