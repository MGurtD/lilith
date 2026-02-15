using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class PurchaseOrderDetailBuilder : IEntityTypeConfiguration<PurchaseOrderDetail>
    {
        public const string TABLE_NAME = "PurchaseOrderDetails";
        public void Configure(EntityTypeBuilder<PurchaseOrderDetail> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Description)
                .HasColumnType("varchar")
                .HasMaxLength(400);
            builder
                .Property(b => b.Quantity)
                .IsRequired()
                .HasColumnType("int")
                .HasDefaultValue(0);
            builder
                .Property(b => b.ReceivedQuantity)
                .IsRequired()
                .HasColumnType("int")
                .HasDefaultValue(0);
            builder
                .Property(b => b.ExpectedReceiptDate)
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.UnitPrice)
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

            builder.ToTable($"{TABLE_NAME}");
        }
    }
}
