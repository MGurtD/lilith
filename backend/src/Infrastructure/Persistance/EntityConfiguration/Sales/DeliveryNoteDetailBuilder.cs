using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class DeliveryNoteDetailBuilder : IEntityTypeConfiguration<DeliveryNoteDetail>
    {
        public const string TABLE_NAME = "DeliveryNoteDetails";

        public void Configure(EntityTypeBuilder<DeliveryNoteDetail> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
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
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.TotalCost)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Amount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.IsInvoiced)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnType("boolean");

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder.ToTable(TABLE_NAME);
        }
    }
}
