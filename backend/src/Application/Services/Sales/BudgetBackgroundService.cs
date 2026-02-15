using Application.Services.Sales;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Application.Contracts;

namespace Application.Services.Sales;

public class BudgetBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var budgetService = scope.ServiceProvider.GetRequiredService<IBudgetService>();

                // Llamada a la función de IBudgetService que necesitas
                await budgetService.RejectOutdatedBudgets();
            }

            await Task.Delay(TimeSpan.FromHours(8), stoppingToken); // Configura el intervalo deseado
        }
    }
}





