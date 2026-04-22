using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class PurchaseRateBuilder : IEntityTypeConfiguration<PurchaseRate>
    {
        public void Configure(EntityTypeBuilder<PurchaseRate> builder)
        {
            builder.HasQueryFilter(e => !e.Disabled);
            builder.Property(e => e.Name).HasMaxLength(250).IsRequired();

            builder.HasOne(e => e.Supplier)
                   .WithMany()
                   .HasForeignKey(e => e.SupplierId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Details)
                   .WithOne(d => d.PurchaseRate)
                   .HasForeignKey(d => d.PurchaseRateId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
