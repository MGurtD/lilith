using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Sales;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class SalesInvoiceDetailBuilder : IEntityTypeConfiguration<SalesInvoiceDetail>
    {
        public const string TABLE_NAME = "SalesInvoiceDetails";

        public void Configure(EntityTypeBuilder<SalesInvoiceDetail> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Quantity)
                .IsRequired()
                .HasColumnType("integer");
            builder
                .Property(b => b.UnitCost)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.TotalCost)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.Amount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder.ToTable(TABLE_NAME);
        }

    }
}
