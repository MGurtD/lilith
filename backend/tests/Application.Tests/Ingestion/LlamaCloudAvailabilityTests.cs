using System.Net;
using Application.Contracts;
using Application.Tests.TestSupport;
using Infrastructure.Ingestion;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Application.Tests.Ingestion;

/// <summary>
/// Unit tests for the invoice import feature flag of <see cref="LlamaCloudInvoiceExtractor"/> — issue #78.
/// </summary>
public class LlamaCloudAvailabilityTests
{
    [Fact]
    public async Task Is_unavailable_without_key_project_or_url()
    {
        var handler = new StubHandler(HttpStatusCode.OK);

        Assert.False(await Sut(handler, new IngestionSettings { ApiKey = "", ProjectId = "p" }).IsAvailableAsync());
        Assert.False(await Sut(handler, new IngestionSettings { ApiKey = "k", ProjectId = "" }).IsAvailableAsync());
        Assert.False(await Sut(handler, new IngestionSettings { ApiKey = "k", ProjectId = "p", BaseUrl = "" }).IsAvailableAsync());
        Assert.False(await Sut(handler, null).IsAvailableAsync());
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task Is_available_when_the_provider_accepts_the_key_and_caches_the_answer()
    {
        var handler = new StubHandler(HttpStatusCode.OK);
        var settings = Settings();

        Assert.True(await Sut(handler, settings).IsAvailableAsync());
        Assert.True(await Sut(handler, settings).IsAvailableAsync());
        Assert.Equal(1, handler.Calls);
        Assert.Equal("/api/v1/projects", handler.LastPath);
    }

    [Fact]
    public async Task Is_unavailable_when_the_provider_rejects_the_key()
    {
        var handler = new StubHandler(HttpStatusCode.Unauthorized);

        Assert.False(await Sut(handler, Settings()).IsAvailableAsync());
    }

    [Fact]
    public async Task Is_unavailable_when_the_provider_is_unreachable()
    {
        var handler = new StubHandler(null);

        Assert.False(await Sut(handler, Settings()).IsAvailableAsync());
    }

    // A unique key per test keeps the process-wide availability cache isolated.
    private static IngestionSettings Settings() => new()
    {
        ApiKey = $"llx-test-{Guid.NewGuid()}",
        ProjectId = Guid.NewGuid().ToString(),
        BaseUrl = "https://llamacloud.test",
    };

    private static LlamaCloudInvoiceExtractor Sut(StubHandler handler, IngestionSettings? settings) =>
        new(
            new HttpClient(handler) { BaseAddress = new Uri("https://llamacloud.test") },
            Options.Create(new AppSettings { Ingestion = settings }),
            new FormattingLocalizationService(),
            NullLogger<LlamaCloudInvoiceExtractor>.Instance);

    /// <summary>Answers every request with the given status, or throws when null (unreachable).</summary>
    private sealed class StubHandler(HttpStatusCode? status) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public string? LastPath { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            LastPath = request.RequestUri?.AbsolutePath;
            return status is { } code
                ? Task.FromResult(new HttpResponseMessage(code))
                : throw new HttpRequestException("unreachable");
        }
    }
}
