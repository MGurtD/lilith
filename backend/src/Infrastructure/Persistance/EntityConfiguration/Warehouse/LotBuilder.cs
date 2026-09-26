using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Warehouse;

namespace Infrastructure.Persistance.EntityConfiguration.Warehouse
{
    public class LotBuilder : IEntityTypeConfiguration<Lot>
    {
        public void Configure(EntityTypeBuilder<Lot> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Code)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .HasDefaultValue(string.Empty);
            builder
                .Property(b => b.SupplierLotCode)
                .HasColumnType("varchar")
                .HasMaxLength(100);
            builder
                .Property(b => b.RemainingQuantity)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE)
                .HasDefaultValue(0);
            builder
                .Property(b => b.Comment)
                .HasColumnType("varchar")
                .HasMaxLength(500);

            builder
                .HasOne(b => b.Reference)
                .WithMany()
                .HasForeignKey(b => b.ReferenceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unic mentre el lot esta obert; el lot buit ("") mai es tanca i pot repetir-se per referencia
            builder
                .HasIndex(b => new { b.ReferenceId, b.Code })
                .HasFilter("\"ClosedDate\" IS NULL AND \"Code\" <> ''")
                .IsUnique();
        }
    }
}
