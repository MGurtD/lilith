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
/// Uses AddHttpClient&lt;TInterface, TService&gt;() — the HttpClient is built by the factory
/// so BaseAddress and Timeout are configurable from Ingestion__* env-vars.
/// POC-1 decision: structured path via /api/parsing/upload + /api/extraction/run.
/// </summary>
public class LlamaParseInvoiceIngestionService : IInvoiceIngestionService
{
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

        // Step 1: upload the PDF to get a file_id
        var fileId = await UploadPdfAsync(pdfStream, fileName, ct);

        // Step 2: run structured extraction with an invoice schema
        var extraction = await RunExtractionAsync(fileId, ct);

        // Step 3: map to application DTO + resolve Tax rows
        return _mapper.Map(extraction.Data);
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

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/parsing/upload")
        {
            Content = form,
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", _settings!.ApiKey);

        using var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);

        var upload = await response.Content.ReadFromJsonAsync<LlamaParseUploadResponse>(cancellationToken: ct);
        if (upload == null || string.IsNullOrWhiteSpace(upload.Id))
        {
            _logger.LogError("LlamaParse /api/parsing/upload returned no file_id.");
            throw new IngestionException(
                IngestionFailureKind.ProviderUnparseable,
                _localizationService.GetLocalizedString("ProviderUnparseable"));
        }
        return upload.Id;
    }

    private async Task<LlamaParseExtractionResponse> RunExtractionAsync(
        string fileId,
        CancellationToken ct)
    {
        // JSON schema describing supplier-invoice fields. LlamaParse uses this
        // to drive field extraction; the response is then validated against it.
        var schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
                ["invoice_number"] = new { type = "string" },
                ["issue_date"] = new { type = "string", format = "date" },
                ["supplier"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["vat_number"] = new { type = "string" },
                        ["name"] = new { type = "string" },
                    },
                },
                ["totals"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["base_amount"] = new { type = "number" },
                        ["transport_amount"] = new { type = "number" },
                        ["discount_percentage"] = new { type = "number" },
                        ["extra_tax_percentage"] = new { type = "number" },
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
                        ["lines"] = new { type = "array" },
                    },
                },
            },
        };

        var body = new { file_id = fileId, schema };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/extraction/run")
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", _settings!.ApiKey);

        using var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);

        var extraction = await response.Content.ReadFromJsonAsync<LlamaParseExtractionResponse>(cancellationToken: ct);
        if (extraction == null)
        {
            _logger.LogError("LlamaParse /api/extraction/run returned an empty body.");
            throw new IngestionException(
                IngestionFailureKind.ProviderUnparseable,
                _localizationService.GetLocalizedString("ProviderUnparseable"));
        }
        return extraction;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        var status = (int)response.StatusCode;
        var body = await response.Content.ReadAsStringAsync(ct);

        if (status == 401 || status == 403)
        {
            _logger.LogError("LlamaParse auth failed. Status: {Status}", status);
            throw new IngestionException(
                IngestionFailureKind.ProviderAuthFailed,
                _localizationService.GetLocalizedString("ProviderAuthFailed"));
        }

        if (status == 422)
        {
            _logger.LogError("LlamaParse returned 422 unparseable. Body: {Body}", body);
            throw new IngestionException(
                IngestionFailureKind.ProviderUnparseable,
                _localizationService.GetLocalizedString("ProviderUnparseable"));
        }

        _logger.LogError("LlamaParse unavailable. Status: {Status}, Body: {Body}", status, body);
        throw new IngestionException(
            IngestionFailureKind.ProviderUnavailable,
            _localizationService.GetLocalizedString("ProviderUnavailable"));
    }
}