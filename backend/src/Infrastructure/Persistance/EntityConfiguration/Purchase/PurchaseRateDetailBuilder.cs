using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class PurchaseRateDetailBuilder : IEntityTypeConfiguration<PurchaseRateDetail>
    {
        public void Configure(EntityTypeBuilder<PurchaseRateDetail> builder)
        {
            builder.HasQueryFilter(e => !e.Disabled);
            builder.Property(e => e.Price).HasPrecision(18, 4);
            builder.Property(e => e.From).HasPrecision(18, 4);
            builder.Property(e => e.To).HasPrecision(18, 4);

            builder.HasOne(e => e.Reference)
                   .WithMany()
                   .HasForeignKey(e => e.ReferenceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
