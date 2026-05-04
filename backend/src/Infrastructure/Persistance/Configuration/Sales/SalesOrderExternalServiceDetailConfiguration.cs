using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.Configuration.Sales;

public class SalesOrderExternalServiceDetailConfiguration : IEntityTypeConfiguration<SalesOrderExternalServiceDetail>
{
    public void Configure(EntityTypeBuilder<SalesOrderExternalServiceDetail> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Weight).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Volume).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,4)");

        builder.HasOne(x => x.SalesOrderDetail)
            .WithMany()
            .HasForeignKey(x => x.SalesOrderDetailId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
