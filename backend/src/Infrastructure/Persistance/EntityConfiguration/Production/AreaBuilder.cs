using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class AreaBuilder : IEntityTypeConfiguration<Area>
    {
        public void Configure(EntityTypeBuilder<Area> builder)
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
                .HasKey(b => b.Id)
                .HasName("PK_Area");
            builder.HasIndex(builder => builder.Name, "UK_Area_Name");

            builder
                .Property(b => b.IsVisibleInPlant)
                .HasColumnType("bool")
                .HasDefaultValue(true);

            builder.ToTable("Areas");
        }
    }
}
