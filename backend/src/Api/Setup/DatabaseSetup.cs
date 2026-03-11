using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Api.Setup;

public static class DatabaseSetup
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, string connectionString)
    {
        // Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        return services;
    }
}
