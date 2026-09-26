using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Contracts;
using Application.Contracts.Ingestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Ingestion;

/// <summary>
/// Extracts supplier invoices with the LlamaCloud extract API
/// (developers.llamaindex.ai/llamaparse/extract/api):
///   1. POST /api/v1/beta/files (multipart, purpose=extract) → file id
///   2. POST /api/v2/extract?project_id= with the invoice schema → job id
///   3. GET  /api/v2/extract/{jobId}?expand=extract_metadata until a terminal status
/// Invoice content is never logged.
/// </summary>
public class LlamaCloudInvoiceExtractor(
    HttpClient httpClient,
    IOptions<AppSettings> options,
    ILocalizationService localizationService,
    ILogger<LlamaCloudInvoiceExtractor> logger) : IInvoiceExtractor
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private const int MaxLoggedErrorLength = 500;

    public async Task<ExtractedInvoice> ExtractAsync(Stream pdfStream, string fileName, CancellationToken ct = default)
    {
        var settings = options.Value.Ingestion;
        if (settings is null || string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.ProjectId))
        {
            logger.LogWarning("Invoice ingestion is not configured (Ingestion:ApiKey / Ingestion:ProjectId).");
            throw Failure(IngestionFailureKind.NotConfigured);
        }

        var elapsed = Stopwatch.StartNew();
        var fileId = await UploadAsync(settings, pdfStream, fileName, ct);
        var jobId = await CreateJobAsync(settings, fileId, ct);
        var job = await PollJobAsync(settings, jobId, elapsed, ct);

        logger.LogInformation("LlamaCloud extract job {JobId} completed in {Elapsed:F1}s.", jobId, elapsed.Elapsed.TotalSeconds);
        return LlamaCloudExtractionMapper.Map(job.ExtractResult, job.ExtractMetadata);
    }

    private async Task<string> UploadAsync(IngestionSettings settings, Stream pdfStream, string fileName, CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();
        var file = new StreamContent(pdfStream);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(file, "file", fileName);
        form.Add(new StringContent("extract"), "purpose");

        using var request = Request(settings, HttpMethod.Post, "/api/v1/beta/files");
        request.Content = form;
        using var response = await SendAsync(request, "upload", ct);

        var upload = await response.Content.ReadFromJsonAsync<IdResponse>(ct);
        return string.IsNullOrWhiteSpace(upload?.Id)
            ? throw Unparseable("upload returned no file id")
            : upload.Id;
    }

    private async Task<string> CreateJobAsync(IngestionSettings settings, string fileId, CancellationToken ct)
    {
        var body = new
        {
            file_input = fileId,
            configuration = new
            {
                tier = settings.Tier,
                version = settings.Version,
                extraction_target = "per_doc",
                data_schema = LlamaCloudExtractionMapper.DataSchema,
                confidence_scores = settings.ConfidenceScores,
                cite_sources = false,
            },
        };

        using var request = Request(settings, HttpMethod.Post, $"/api/v2/extract?project_id={Uri.EscapeDataString(settings.ProjectId)}");
        request.Content = JsonContent.Create(body);
        using var response = await SendAsync(request, "create_job", ct);

        var job = await response.Content.ReadFromJsonAsync<IdResponse>(ct);
        return string.IsNullOrWhiteSpace(job?.Id)
            ? throw Unparseable("create job returned no job id")
            : job.Id;
    }

    private async Task<JobResponse> PollJobAsync(IngestionSettings settings, string jobId, Stopwatch elapsed, CancellationToken ct)
    {
        // Leave a margin inside the HttpClient timeout for the last round trip.
        var budget = TimeSpan.FromSeconds(Math.Max(5, settings.TimeoutSeconds - 5));
        var url = $"/api/v2/extract/{Uri.EscapeDataString(jobId)}" +
                  $"?project_id={Uri.EscapeDataString(settings.ProjectId)}&expand=extract_metadata";

        while (true)
        {
            if (elapsed.Elapsed > budget)
            {
                logger.LogWarning("LlamaCloud extract job {JobId} exceeded the {Budget}s budget.", jobId, budget.TotalSeconds);
                throw Failure(IngestionFailureKind.Unavailable);
            }

            using var request = Request(settings, HttpMethod.Get, url);
            using var response = await SendAsync(request, "poll_job", ct);
            var job = await response.Content.ReadFromJsonAsync<JobResponse>(ct)
                ?? throw Unparseable("poll returned an empty body");

            switch (job.Status.ToUpperInvariant())
            {
                case "COMPLETED":
                    return job;
                case "FAILED":
                case "CANCELLED":
                    throw Unparseable($"job {jobId} finished with status {job.Status}");
            }

            await Task.Delay(PollInterval, ct);
        }
    }

    private static HttpRequestMessage Request(IngestionSettings settings, HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        return request;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, string step, CancellationToken ct)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !ct.IsCancellationRequested))
        {
            logger.LogWarning(ex, "LlamaCloud request failed at step {Step}.", step);
            throw Failure(IngestionFailureKind.Unavailable);
        }

        if (response.IsSuccessStatusCode) return response;

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (body.Length > MaxLoggedErrorLength) body = body[..MaxLoggedErrorLength];
            logger.LogWarning("LlamaCloud returned {Status} at step {Step}: {Body}", (int)response.StatusCode, step, body);

            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => Failure(IngestionFailureKind.AuthFailed),
                // A 4xx creating the job means our schema, project or configuration is wrong.
                _ when step == "create_job" && (int)response.StatusCode is >= 400 and < 500 => Failure(IngestionFailureKind.ConfigError),
                HttpStatusCode.UnprocessableEntity or HttpStatusCode.BadRequest => Failure(IngestionFailureKind.Unparseable),
                _ => Failure(IngestionFailureKind.Unavailable),
            };
        }
    }

    private IngestionException Unparseable(string reason)
    {
        logger.LogWarning("LlamaCloud response could not be used: {Reason}.", reason);
        return Failure(IngestionFailureKind.Unparseable);
    }

    private IngestionException Failure(IngestionFailureKind kind) =>
        new(kind, localizationService.GetLocalizedString($"InvoiceIngestion{kind}"));

    private sealed class IdResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private sealed class JobResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("extract_result")]
        public JsonElement ExtractResult { get; set; }

        [JsonPropertyName("extract_metadata")]
        public JsonElement? ExtractMetadata { get; set; }
    }
}
