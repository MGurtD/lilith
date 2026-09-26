using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class SalesOrderDetailPhaseProfitBuilder : IEntityTypeConfiguration<SalesOrderDetailPhaseProfit>
    {
        public const string TABLE_NAME = "SalesOrderDetailPhaseProfits";
        public void Configure(EntityTypeBuilder<SalesOrderDetailPhaseProfit> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.SalesOrderDetailId)
                .IsRequired()
                .HasColumnType("uuid");
            builder
                .Property(b => b.WorkMasterPhaseDetailId)
                .IsRequired()
                .HasColumnType("uuid");
            builder
                .Property(b => b.ProfitPercentage)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .HasOne(b => b.SalesOrderDetail)
                .WithMany(d => d.PhaseProfits)
                .HasForeignKey(b => b.SalesOrderDetailId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.ToTable(TABLE_NAME);
        }
    }
}
