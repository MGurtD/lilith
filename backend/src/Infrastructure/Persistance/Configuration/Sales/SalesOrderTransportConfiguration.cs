using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.Configuration.Sales;

public class SalesOrderTransportConfiguration : IEntityTypeConfiguration<SalesOrderTransport>
{
    public void Configure(EntityTypeBuilder<SalesOrderTransport> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Weight).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Volume).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Distance).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Price).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Description).HasMaxLength(250);
        builder.Property(x => x.Destination).HasMaxLength(250);

        builder.HasOne<SalesOrderHeader>()
            .WithMany(b => b.Transports)
            .HasForeignKey(x => x.SalesOrderHeaderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
