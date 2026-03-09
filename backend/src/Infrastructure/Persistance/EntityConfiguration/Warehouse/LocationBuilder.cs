using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Warehouse;

namespace Infrastructure.Persistance.EntityConfiguration.Warehouse
{
    public class LocationBuilder : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.LocationType)
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_Location");
            builder.HasIndex(builder => new { builder.Name, builder.WarehouseId }, "UK_Location_Warehouse");

            builder.ToTable("Locations");
        }
    }
}