using Application.Contracts;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api.Setup;

public static class OpenTelemetrySetup
{
    public static WebApplicationBuilder AddOpenTelemetrySetup(
        this WebApplicationBuilder builder,
        OpenTelemetrySettings settings)
    {
        var resource = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: settings.ServiceName,
                serviceVersion: settings.ServiceVersion);

        var authHeader = settings.GetAuthorizationHeader();

        void ConfigureOtlpExporter(OtlpExporterOptions options)
        {
            options.Endpoint = new Uri(settings.Endpoint);
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
            options.Headers = $"Authorization={authHeader}";
        }

        // Logging
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.SetResourceBuilder(resource);
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.AddOtlpExporter(ConfigureOtlpExporter);
        });

        // Tracing & Metrics
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(ConfigureOtlpExporter);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(ConfigureOtlpExporter);
            });

        return builder;
    }
}
