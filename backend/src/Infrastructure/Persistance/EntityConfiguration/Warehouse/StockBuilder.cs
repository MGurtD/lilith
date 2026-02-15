using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Warehouse;

namespace Infrastructure.Persistance.EntityConfiguration.Warehouse
{
    public class StockBuilder : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Quantity)
                .IsRequired()
                .HasColumnType("integer");                
            builder
                .Property(b => b.Width)
                .IsRequired()                
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Length)
                .IsRequired()                
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Height)
                .IsRequired()                
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Diameter)
                .IsRequired()                
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Thickness)
                .IsRequired()                
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_Stocks");
            builder.HasIndex(builder => new { builder.LocationId, builder.ReferenceId }, "idx_Location_Reference");

            builder.ToTable("Stocks");

        }
    }

}