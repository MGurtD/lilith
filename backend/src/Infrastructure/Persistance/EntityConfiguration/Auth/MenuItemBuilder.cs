using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Auth;

namespace Infrastructure.Persistance.EntityConfiguration.Auth
{
    public class MenuItemBuilder : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.ConfigureBase();

            builder.Property(b => b.Key)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder.Property(b => b.Title)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder.Property(b => b.Icon)
                .HasColumnType("varchar")
                .HasMaxLength(100);

            builder.Property(b => b.Route)
                .HasColumnType("varchar")
                .HasMaxLength(500);

            builder.Property(b => b.SortOrder)
                .IsRequired()
                .HasColumnType("int")
                .HasDefaultValue(0);

            builder.Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder.HasIndex(b => b.Key, "UK_MenuItem_Key");

            builder.HasOne(b => b.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(b => b.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasKey(b => b.Id).HasName("PK_MenuItem");
            builder.ToTable("MenuItems");
        }
    }
}
