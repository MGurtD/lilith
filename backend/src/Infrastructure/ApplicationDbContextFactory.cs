using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Infrastructure.Persistance;
using Microsoft.Extensions.Configuration;

namespace Infrastructure
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var connectionString = GetConnectionString();
            if (connectionString == string.Empty)
            {
                throw new Exception("'ConnectionString' configuration is not provided");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets("Lilith.Backend")
                .AddEnvironmentVariables()
                .Build();

            return config["ConnectionStrings:Default"] ?? "";
        }
    }
}
