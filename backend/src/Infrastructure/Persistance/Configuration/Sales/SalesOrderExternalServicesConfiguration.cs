using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.Configuration.Sales;

public class SalesOrderExternalServicesConfiguration : IEntityTypeConfiguration<SalesOrderExternalServices>
{
    public void Configure(EntityTypeBuilder<SalesOrderExternalServices> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Description).HasMaxLength(250);
        builder.Property(x => x.Weight).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Volume).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)");
        builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,4)");

        builder.HasOne<SalesOrderHeader>()
            .WithMany(b => b.ExternalServices)
            .HasForeignKey(x => x.SalesOrderHeaderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Details)
            .WithOne(d => d.SalesOrderExternalService)
            .HasForeignKey(d => d.SalesOrderExternalServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
