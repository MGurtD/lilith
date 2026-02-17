using Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services.Production;

public class ProductionPartGeneratorService(
    IProductionPartChannel channel,
    IServiceScopeFactory scopeFactory,
    ILogger<ProductionPartGeneratorService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ProductionPartGeneratorService iniciat");

        await foreach (var request in channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider
                    .GetRequiredService<IProductionPartGeneratorHandler>();
                await handler.GenerateFromPhaseClose(request);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error generant tiquets de produccio per la fase {PhaseId}",
                    request.WorkOrderPhaseId);
            }
        }

        logger.LogInformation("ProductionPartGeneratorService aturat");
    }
}
