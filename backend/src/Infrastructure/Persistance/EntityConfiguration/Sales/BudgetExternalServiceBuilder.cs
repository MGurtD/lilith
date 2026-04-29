using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class BudgetExternalServicesBuilder : IEntityTypeConfiguration<BudgetExternalServices>
    {
        public const string TABLE_NAME = "BudgetExternalServices";
        public void Configure(EntityTypeBuilder<BudgetExternalServices> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.BudgetId)
                .IsRequired()
                .HasColumnType("uuid");
            builder
                .Property(b => b.ReferenceId)
                .IsRequired()
                .HasColumnType("uuid");
            builder
                .Property(b => b.SupplierId)
                .IsRequired()
                .HasColumnType("uuid");
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
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
            builder
                .Property(b => b.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder.ToTable(TABLE_NAME);
        }
    }
}