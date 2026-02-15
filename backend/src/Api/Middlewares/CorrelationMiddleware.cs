namespace Api.Middlewares
{
    /// <summary>
    /// Middleware that generates or extracts correlation IDs for request tracking.
    /// Adds X-Correlation-ID header to all responses and stores in HttpContext.Items for logging.
    /// </summary>
    public class CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private const string CorrelationIdKey = "CorrelationId";

        public async Task Invoke(HttpContext context)
        {
            // Try to extract correlation ID from request header, or generate new one
            var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault() 
                ?? Guid.NewGuid().ToString();

            // Store in HttpContext.Items for access by other middleware/controllers
            context.Items[CorrelationIdKey] = correlationId;

            // Add to response headers for client tracking
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
                {
                    context.Response.Headers[CorrelationIdHeader] = correlationId;
                }
                return Task.CompletedTask;
            });

            logger.LogDebug("Request correlation ID: {CorrelationId}", correlationId);

            await next(context);
        }
    }
}
