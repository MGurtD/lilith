using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class SupplierReferenceBuilder : IEntityTypeConfiguration<SupplierReference>
    {
        public const string TABLE_NAME = "SupplierReferences";

        public void Configure(EntityTypeBuilder<SupplierReference> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.SupplierCode)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.SupplierDescription)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(2000);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(e => new { e.ReferenceId, e.SupplierId })
                .IsUnique();

            builder.ToTable(TABLE_NAME);
        }

    }
}
