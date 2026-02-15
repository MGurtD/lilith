using Api.Exceptions;
using Api.Models;
using Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace Api.Middlewares
{
    /// <summary>
    /// Enhanced error handling middleware with structured logging, localization, and RFC 7807 ProblemDetails responses.
    /// </summary>
    public class ErrorHandlerMiddleware(
        RequestDelegate next,
        ILoggerFactory loggerFactory,
        ILocalizationService localizationService,
        IWebHostEnvironment environment)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<ErrorHandlerMiddleware>();

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                await HandleExceptionAsync(context, exception, stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, long elapsedMs)
        {
            // Determine status code and localization key
            var (statusCode, localizationKey) = MapExceptionToStatusCode(exception);

            // Extract correlation ID
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            // Extract user context
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? context.User?.FindFirst("id")?.Value 
                ?? "anonymous";
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Check if this is a health check endpoint (exclude from detailed logging)
            var isHealthCheck = IsHealthCheckEndpoint(context.Request.Path);

            // Build structured logging properties
            var logProperties = new Dictionary<string, object>
            {
                { "CorrelationId", correlationId },
                { "UserId", userId },
                { "UserAgent", userAgent },
                { "RemoteIp", remoteIp },
                { "RequestPath", context.Request.Path.Value ?? string.Empty },
                { "RequestMethod", context.Request.Method },
                { "StatusCode", statusCode },
                { "ElapsedMs", elapsedMs },
                { "ExceptionType", exception.GetType().Name }
            };

            // Choose appropriate log level based on status code
            var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;

            // Log with structured properties
            _logger.Log(
                logLevel,
                exception,
                "HTTP {StatusCode} - {Method} {Path} | User: {UserId} | TraceId: {CorrelationId} | Elapsed: {ElapsedMs}ms | Exception: {ExceptionType}",
                statusCode,
                context.Request.Method,
                context.Request.Path,
                userId,
                correlationId,
                elapsedMs,
                exception.GetType().Name
            );

            // Build error response
            var errorResponse = BuildErrorResponse(
                context,
                exception,
                statusCode,
                localizationKey,
                correlationId
            );

            // Set response properties
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // Add Cache-Control header for 5xx errors (prevent caching of error responses)
            if (statusCode >= 500)
            {
                context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
                context.Response.Headers.Pragma = "no-cache";
                context.Response.Headers.Expires = "0";
            }

            // Serialize and write response
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var result = JsonSerializer.Serialize(errorResponse, options);
            await context.Response.WriteAsync(result);
        }

        private static (int StatusCode, string LocalizationKey) MapExceptionToStatusCode(Exception exception)
        {
            return exception switch
            {
                ApiException => ((int)HttpStatusCode.BadRequest, "ErrorResponse.BadRequest"),
                KeyNotFoundException => ((int)HttpStatusCode.NotFound, "ErrorResponse.NotFound"),
                UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "ErrorResponse.Unauthorized"),
                ArgumentNullException => ((int)HttpStatusCode.BadRequest, "ErrorResponse.BadRequest"),
                ArgumentException => ((int)HttpStatusCode.BadRequest, "ErrorResponse.BadRequest"),
                InvalidOperationException => ((int)HttpStatusCode.BadRequest, "ErrorResponse.BadRequest"),
                DbUpdateException => ((int)HttpStatusCode.Conflict, "ErrorResponse.Conflict"),
                _ => ((int)HttpStatusCode.InternalServerError, "ErrorResponse.InternalServerError")
            };
        }

        private ErrorResponse BuildErrorResponse(
            HttpContext context,
            Exception exception,
            int statusCode,
            string localizationKey,
            string correlationId)
        {
            var errors = new List<string> { exception.Message };
            if (exception.InnerException != null)
            {
                errors.Add(exception.InnerException.Message);
            }

            // Get localized title
            var title = localizationService.GetLocalizedString(localizationKey);

            var errorResponse = new ErrorResponse
            {
                Type = $"https://httpstatuses.com/{statusCode}",
                Title = title,
                Status = statusCode,
                Detail = environment.IsDevelopment() ? exception.Message : title,
                Instance = context.Request.Path,
                TraceId = correlationId,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };

            return errorResponse;
        }

        private static bool IsHealthCheckEndpoint(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;
            return pathValue.Contains("/health") || 
                   pathValue.Contains("/ready") || 
                   pathValue.Contains("/live");
        }
    }
}
