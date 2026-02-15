using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.EntityConfiguration.Warehouse
{
    public class WarehouseBuilder : IEntityTypeConfiguration<Domain.Entities.Warehouse.Warehouse>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Warehouse.Warehouse> builder)
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
                .HasOne(e => e.DefaultLocation)
                .WithMany()
                .HasForeignKey(e => e.DefaultLocationId);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_Warehouse");
            builder.HasIndex(builder => builder.Name, "UK_Warehouse_Name");

            builder.ToTable("Warehouses");
        }
    }
}

