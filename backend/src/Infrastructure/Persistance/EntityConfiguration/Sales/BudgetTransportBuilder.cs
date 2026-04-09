using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class BudgetTransportBuilder : IEntityTypeConfiguration<BudgetTransport>
    {
        public const string TABLE_NAME = "BudgetTransports";
        public void Configure(EntityTypeBuilder<BudgetTransport> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.BudgetId)
            .IsRequired()
            .HasColumnType("uuid");
        builder
            .Property(b => b.TransportRateDetailId)
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
            .Property(b => b.Distance)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.Price)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
        builder
            .Property(b => b.Description)
            .HasColumnType("varchar")
            .HasMaxLength(250);
        builder
            .Property(b => b.Destination)
            .HasColumnType("varchar")
            .HasMaxLength(250);
        builder.ToTable(TABLE_NAME);
        }
    }
}