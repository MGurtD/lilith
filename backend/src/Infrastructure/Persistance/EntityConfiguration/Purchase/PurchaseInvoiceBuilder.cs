using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class PurchaseInvoiceBuilder : IEntityTypeConfiguration<PurchaseInvoice>
    {
        public const string TABLE_NAME = "PurchaseInvoices";

        public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Number)
                .IsRequired()
                .HasDefaultValue("0")
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.SupplierNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(25);
            builder
                .Property(b => b.PurchaseInvoiceDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.BaseAmount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.TransportAmount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.Subtotal)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.GrossAmount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.NetAmount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.DiscountPercentage)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.DiscountAmount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);

            builder
                .Property(b => b.ExtraTaxAmount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasDefaultValue(0)
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);

            builder
                .Property(b => b.ExtraTaxPercentatge)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnType("integer");

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(builder => new { builder.ExerciceId, builder.Number }, $"UK_{TABLE_NAME}")
                .IsUnique();
            builder
                .HasIndex(builder => builder.PurchaseInvoiceDate, $"IX_{TABLE_NAME}_PurchaseDate")
                .IsDescending();

            builder.ToTable(TABLE_NAME);
        }

    }
}
