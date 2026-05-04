using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class BudgetExternalServiceDetailBuilder : IEntityTypeConfiguration<BudgetExternalServiceDetail>
    {
        public const string TABLE_NAME = "BudgetExternalServiceDetails";
        public void Configure(EntityTypeBuilder<BudgetExternalServiceDetail> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.BudgetExternalServiceId)
                .IsRequired()
                .HasColumnType("uuid");
            builder
                .Property(b => b.BudgetDetailId)
                .IsRequired()
                .HasColumnType("uuid");
            builder
                .Property(b => b.Weight)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Volume)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Quantity)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder.ToTable(TABLE_NAME);
        }
    }
}
