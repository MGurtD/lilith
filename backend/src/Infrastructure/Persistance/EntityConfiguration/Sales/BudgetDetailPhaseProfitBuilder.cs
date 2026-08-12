using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class BudgetDetailPhaseProfitBuilder : IEntityTypeConfiguration<BudgetDetailPhaseProfit>
    {
        public const string TABLE_NAME = "BudgetDetailPhaseProfits";
        public void Configure(EntityTypeBuilder<BudgetDetailPhaseProfit> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.BudgetDetailId)
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
                .HasOne(b => b.BudgetDetail)
                .WithMany(d => d.PhaseProfits)
                .HasForeignKey(b => b.BudgetDetailId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.ToTable(TABLE_NAME);
        }
    }
}
