using Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services.Production;

public class WorkOrderPhaseCloseService(
    IWorkOrderPhaseCloseChannel channel,
    IServiceScopeFactory scopeFactory,
    ILogger<WorkOrderPhaseCloseService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("WorkOrderPhaseCloseService iniciat");

        await foreach (var request in channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider
                    .GetRequiredService<IWorkOrderPhaseCloseHandler>();
                await handler.HandlePhaseClose(request);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error processant tancament de fase {PhaseId}",
                    request.WorkOrderPhaseId);
            }
        }

        logger.LogInformation("WorkOrderPhaseCloseService aturat");
    }
}
