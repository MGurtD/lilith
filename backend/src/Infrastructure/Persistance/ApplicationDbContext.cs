using Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance
{
    public static class ApplicationDbContextConstants
    {
        public const int DECIMAL_PRECISION = 18;
        public const int DECIMAL_SCALE = 4;
        public const int PRICE_DECIMAL_SCALE = 2;
        public const string TIMESTAMP_WITHOUT_TIMEZONE = "timestamp without time zone";
    }
    public static class ApplicationDbContextSchemas
    {
        public const string PUBLIC = "public";
        public const string DATA = "data";
    }

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Domain.Entities.Sales.SalesOrderTransport> SalesOrderTransports { get; set; }
        public DbSet<Domain.Entities.Sales.SalesOrderExternalServices> SalesOrderExternalServices { get; set; }
        public DbSet<Domain.Entities.Sales.SalesOrderExternalServiceDetail> SalesOrderExternalServiceDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            BuildApplicationViewMappings(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            MasterData.MasterDataForeignKeys.Restrict(builder.Model);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            try
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch (DbUpdateException exception) when (MasterData.MasterDataDeleteGuard.DatabaseRefusal(Model, exception) is { } refusal)
            {
                throw refusal;
            }
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (DbUpdateException exception) when (MasterData.MasterDataDeleteGuard.DatabaseRefusal(Model, exception) is { } refusal)
            {
                throw refusal;
            }
        }

        private static void BuildApplicationViewMappings(ModelBuilder builder)
        {
           builder
                .Entity<ConsolidatedExpense>()
                .ToView("vw_consolidatedExpenses")
                .HasNoKey();
            builder
               .Entity<SalesInvoiceDetailReport>()
               .ToView("vw_report_salesInvoiceDetails")
               .HasNoKey();
            builder
                .Entity<DetailedWorkOrder>()
                .ToView("vw_detailedworkorder")
                .HasNoKey();
            builder
                .Entity<ProductionCost>()
                .ToView("vw_productioncosts")
                .HasNoKey();
            builder
                .Entity<ConsolidatedIncomes>()
                .ToView("vw_consolidatedIncomes")
                .HasNoKey();
            builder
                .Entity<WorkcenterShiftHistoricalOperator>()
                .ToView("vw_workcentershift_historical_operator")
                .HasNoKey();
            builder
                .Entity<WorkcenterShiftHistorical>()
                .ToView("vw_workcentershift_historical")
                .HasNoKey()
                .Ignore(x => x.Key);
        }

    }
}
