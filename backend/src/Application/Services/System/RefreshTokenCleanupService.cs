using Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services.System;

public class RefreshTokenCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    // Run once per day; expired tokens live for 6 months so daily is sufficient
    private static readonly TimeSpan Interval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RefreshTokenCleanupService started. Interval: {Interval}", Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

                var deleted = await authService.PurgeExpiredRefreshTokens();
                logger.LogInformation("RefreshTokenCleanupService: purged {Count} expired/used/revoked refresh tokens", deleted);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RefreshTokenCleanupService: unhandled error during purge");
            }

            await Task.Delay(Interval, stoppingToken);
        }

        logger.LogInformation("RefreshTokenCleanupService stopped");
    }
}
