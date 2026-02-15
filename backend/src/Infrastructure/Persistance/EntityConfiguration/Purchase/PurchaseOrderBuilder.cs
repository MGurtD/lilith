using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class PurchaseOrderBuilder : IEntityTypeConfiguration<PurchaseOrder>
    {
        public const string TABLE_NAME = "PurchaseOrder";
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Number)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.Date)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            
            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");
            builder
                .HasIndex(b => b.Number)
                .HasDatabaseName($"IX_{TABLE_NAME}_Number")
                .IsUnique();

            builder.ToTable($"{TABLE_NAME}s");

        }
    }
}
