using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Warehouse;

namespace Infrastructure.Persistance.EntityConfiguration.Warehouse
{
    public class StockMovementBuilder : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Quantity)
                .IsRequired()
                .HasColumnType("integer");     
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);             
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
                .Property(b => b.MovementDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
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
                .HasName("PK_StockMovements");
            builder.HasIndex(builder => new { builder.StockId}, "idx_stockid");
            builder.HasIndex(builder => new { builder.MovementType}, "idx_movementtype");
            builder.HasIndex(builder => new { builder.StockId, builder.MovementType}, "idx_stockid_movementtype");

            builder.ToTable("StockMovements");

        }
    }

}