using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase;

public class PurchaseOrderReceiptDetailBuilder : IEntityTypeConfiguration<PurchaseOrderReceiptDetail>
{
    public const string TABLE_NAME = "PurchaseOrderReceiptDetails";
    public void Configure(EntityTypeBuilder<PurchaseOrderReceiptDetail> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.Quantity)
            .IsRequired()
            .HasColumnType("int")
            .HasDefaultValue(0);
        builder
            .Property(b => b.User)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(500);

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");

        builder.ToTable($"{TABLE_NAME}");
    }
}
