using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Auth;

namespace Infrastructure.Persistance.EntityConfiguration.Auth
{
    public class ProfileMenuItemBuilder : IEntityTypeConfiguration<ProfileMenuItem>
    {
        public void Configure(EntityTypeBuilder<ProfileMenuItem> builder)
        {
            builder.ConfigureBase();

            builder.Property(b => b.IsDefault)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder.Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder.HasOne(b => b.Profile)
                .WithMany(p => p.ProfileMenuItems)
                .HasForeignKey(b => b.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.MenuItem)
                .WithMany(p => p.ProfileMenuItems)
                .HasForeignKey(b => b.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => new { b.ProfileId, b.MenuItemId }, "UK_ProfileMenuItem_Profile_MenuItem");
            builder.HasIndex(b => new { b.ProfileId, b.IsDefault }, "IX_ProfileMenuItem_Profile_IsDefault");

            builder.HasKey(b => b.Id).HasName("PK_ProfileMenuItem");
            builder.ToTable("ProfileMenuItems");
        }
    }
}
