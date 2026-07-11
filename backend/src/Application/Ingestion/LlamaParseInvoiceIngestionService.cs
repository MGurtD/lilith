using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Contracts;
using Application.Contracts.Ingestion;
using Application.Contracts.Persistance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Ingestion;

/// <summary>
/// LlamaParse implementation of IInvoiceIngestionService.
/// Aligned with the current LlamaCloud SaaS API (developers.llamaindex.ai/llamaparse/extract/api):
///   1. Upload PDF → POST /api/v1/beta/files (multipart, purpose=extract) → { id }
///   2. Create extract job → POST /api/v2/extract?project_id={PROJECT_ID}
///        body: { file_input, configuration: { tier, version, extraction_target, data_schema,
///                                            confidence_scores, cite_sources, system_prompt } }
///        → { id, status }
///   3. Poll → GET /api/v2/extract/{jobId}?project_id=...&expand=extract_metadata
///        until status ∈ { COMPLETED, FAILED, CANCELLED }
///   4. Map → LlamaParsePayloadMapper.Map(extraction.ExtractResult)
/// Uses AddHttpClient&lt;TInterface, TService&gt;() — HttpClient built by the factory
/// so BaseAddress and Timeout are configurable from Ingestion__* env-vars.
/// </summary>
public class LlamaParseInvoiceIngestionService : IInvoiceIngestionService
{
    private const int PollIntervalSeconds = 2;

    private static readonly HashSet<string> TerminalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "COMPLETED", "FAILED", "CANCELLED"
    };

    private readonly HttpClient _httpClient;
    private readonly IngestionSettings? _settings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<LlamaParseInvoiceIngestionService> _logger;
    private readonly LlamaParsePayloadMapper _mapper;

    public LlamaParseInvoiceIngestionService(
        HttpClient httpClient,
        IOptions<AppSettings> options,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        ILogger<LlamaParseInvoiceIngestionService> logger,
        LlamaParsePayloadMapper mapper)
    {
        _httpClient = httpClient;
        _settings = options.Value.Ingestion;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IngestPurchaseInvoiceResponse> IngestAsync(
        Stream pdfStream,
        string fileName,
        CancellationToken ct = default)
    {
        if (_settings == null || string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _logger.LogWarning("Ingestion:ApiKey not configured — returning ProviderNotConfigured.");
            throw new IngestionException(
                IngestionFailureKind.ProviderNotConfigured,
                _localizationService.GetLocalizedString("ProviderNotConfigured"));
        }

        if (string.IsNullOrWhiteSpace(_settings.ProjectId))
        {
            _logger.LogWarning("Ingestion:ProjectId not configured — returning ProviderNotConfigured.");
            throw new IngestionException(
                IngestionFailureKind.ProviderNotConfigured,
                _localizationService.GetLocalizedString("ProviderNotConfigured"));
        }

        // Step 1: upload the PDF to get a file_id
        var fileId = await UploadPdfAsync(pdfStream, fileName, ct);

        // Step 2: create extract job → returns job id + initial status
        var jobId = await CreateExtractJobAsync(fileId, ct);

        // Step 3: poll until terminal status, then map result
        var extraction = await PollExtractJobAsync(jobId, ct);
        _logger.LogInformation(
            "LlamaParse extract_result for {FileName}: {RawPayload}",
            fileName,
            System.Text.Json.JsonSerializer.Serialize(extraction.ExtractResult));
        return _mapper.Map(extraction.ExtractResult);
    }

    private async Task<string> UploadPdfAsync(
        Stream pdfStream,
        string fileName,
        CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();
        var streamContent = new StreamContent(pdfStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(streamContent, "file", fileName);
        form.Add(new StringContent("extract"), "purpose");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/beta/files")
        {
            Content = form,
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", _settings!.ApiKey);

        using var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response, "upload", ct);

        var upload = await response.Content.ReadFromJsonAsync<LlamaParseUploadResponse>(cancellationToken: ct);
        if (upload == null || string.IsNullOrWhiteSpace(upload.Id))
        {
            _logger.LogError("LlamaParse /api/v1/beta/files returned no file id.");
            throw new IngestionException(
                IngestionFailureKind.ProviderUnparseable,
                _localizationService.GetLocalizedString("ProviderUnparseable"));
        }
        return upload.Id;
    }

    private async Task<string> CreateExtractJobAsync(string fileId, CancellationToken ct)
    {
        // JSON schema describing supplier-invoice fields. The model returns data that matches this.
        var dataSchema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
                ["invoice_number"] = new { type = "string", description = "Invoice number or code printed on the document" },
                ["issue_date"] = new { type = "string", format = "date", description = "Issue date (YYYY-MM-DD)" },
                ["supplier"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["vat_number"] = new { type = "string", description = "Supplier VAT / tax identification number" },
                        ["name"] = new { type = "string", description = "Supplier legal or trade name" },
                    },
                },
                ["totals"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["base_amount"] = new { type = "number", description = "Net taxable base amount" },
                        ["transport_amount"] = new { type = "number", description = "Transport / shipping charges" },
                        ["discount_percentage"] = new { type = "number", description = "Early-payment or commercial discount (%)" },
                        ["extra_tax_percentage"] = new { type = "number", description = "Extra tax percentage, if any" },
                    },
                },
                ["tax_breakdown"] = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new Dictionary<string, object>
                        {
                            ["tax_rate"] = new { type = "number" },
                            ["base_amount"] = new { type = "number" },
                            ["tax_amount"] = new { type = "number" },
                            ["surcharge_rate"] = new { type = "number" },
                            ["surcharge_amount"] = new { type = "number" },
                            ["confidence"] = new { type = "number" },
                        },
                    },
                },
                ["confidence"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["headers"] = new { type = "object" },
                        ["lines"] = new
                        {
                            type = "array",
                            items = new { type = "number" },
                        },
                    },
                },
            },
        };

        var body = new
        {
            file_input = fileId,
            configuration = new
            {
                tier = _settings!.Tier,
                version = _settings.Version,
                extraction_target = "per_doc",
                data_schema = dataSchema,
                confidence_scores = _settings.ConfidenceScores,
                cite_sources = false,
            },
        };

        var url = $"/api/v2/extract?project_id={Uri.EscapeDataString(_settings.ProjectId)}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response, "create_extract", ct);

        var created = await response.Content.ReadFromJsonAsync<LlamaParseExtractionResponse>(cancellationToken: ct);
        if (created == null || string.IsNullOrWhiteSpace(created.Id))
        {
            _logger.LogError("LlamaParse /api/v2/extract returned no job id.");
            throw new IngestionException(
                IngestionFailureKind.ProviderUnparseable,
                _localizationService.GetLocalizedString("ProviderUnparseable"));
        }
        return created.Id;
    }

    private async Task<LlamaParseExtractionResponse> PollExtractJobAsync(
        string jobId,
        CancellationToken ct)
    {
        // Reserve 5 seconds of the configured budget for the upload + create + final mapping round-trip.
        var pollingBudgetSeconds = Math.Max(5, _settings!.TimeoutSeconds - 5);
        var deadline = Stopwatch.StartNew();

        var url = $"/api/v2/extract/{Uri.EscapeDataString(jobId)}" +
                  $"?project_id={Uri.EscapeDataString(_settings.ProjectId)}" +
                  "&expand=extract_metadata";

        var iteration = 0;
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            if (deadline.Elapsed.TotalSeconds > pollingBudgetSeconds)
            {
                _logger.LogError("LlamaParse polling exceeded {Seconds}s budget.", pollingBudgetSeconds);
                throw new IngestionException(
                    IngestionFailureKind.ProviderUnavailable,
                    _localizationService.GetLocalizedString("ProviderUnavailable"));
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", _settings.ApiKey);

            using var response = await _httpClient.SendAsync(request, ct);
            await EnsureSuccessAsync(response, "poll_extract", ct);

            var current = await response.Content
                .ReadFromJsonAsync<LlamaParseExtractionResponse>(cancellationToken: ct);

            if (current == null || string.IsNullOrWhiteSpace(current.Status))
            {
                _logger.LogError("LlamaParse /api/v2/extract/{{jobId}} returned empty body.");
                throw new IngestionException(
                    IngestionFailureKind.ProviderUnparseable,
                    _localizationService.GetLocalizedString("ProviderUnparseable"));
            }

            iteration++;
            _logger.LogInformation(
                "LlamaParse poll iteration {Iter} → status={Status} (elapsed={Elapsed:F1}s)",
                iteration, current.Status, deadline.Elapsed.TotalSeconds);

            if (TerminalStatuses.Contains(current.Status))
            {
                if (!string.Equals(current.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("LlamaParse extract job finished with status {Status}.", current.Status);
                    throw new IngestionException(
                        IngestionFailureKind.ProviderUnparseable,
                        _localizationService.GetLocalizedString("ProviderUnparseable"));
                }
                return current;
            }

            await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds), ct);
        }
    }

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string step,
        CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        var status = (int)response.StatusCode;
        var body = await response.Content.ReadAsStringAsync(ct);

        if (status == 401 || status == 403)
        {
            _logger.LogError("LlamaParse auth failed at step {Step}. Status: {Status}", step, status);
            throw new IngestionException(
                IngestionFailureKind.ProviderAuthFailed,
                _localizationService.GetLocalizedString("ProviderAuthFailed"));
        }

        if (status == 422)
        {
            // 422 from /files          → document unparseable (existing mapping).
            // 422 from /extract (job)  → invalid schema / config / project_id (new ProviderConfigError).
            var kind = string.Equals(step, "create_extract", StringComparison.OrdinalIgnoreCase)
                ? IngestionFailureKind.ProviderConfigError
                : IngestionFailureKind.ProviderUnparseable;
            var message = kind == IngestionFailureKind.ProviderConfigError
                ? _localizationService.GetLocalizedString("ProviderConfigError")
                : _localizationService.GetLocalizedString("ProviderUnparseable");
            _logger.LogError(
                "LlamaParse returned 422 at step {Step}. Body: {Body}", step, body);
            throw new IngestionException(kind, message);
        }

        _logger.LogError(
            "LlamaParse unavailable at step {Step}. Status: {Status}, Body: {Body}", step, status, body);
        throw new IngestionException(
            IngestionFailureKind.ProviderUnavailable,
            _localizationService.GetLocalizedString("ProviderUnavailable"));
    }
}