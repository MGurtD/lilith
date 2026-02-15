using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Auth;

namespace Infrastructure.Persistance.EntityConfiguration.Auth
{
    public class ProfileBuilder : IEntityTypeConfiguration<Profile>
    {
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.ConfigureBase();

            builder.Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder.Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);

            builder.Property(b => b.IsSystem)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder.Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder.HasIndex(b => b.Name, "UK_Profile_Name");

            builder.HasKey(b => b.Id).HasName("PK_Profile");
            builder.ToTable("Profiles");
        }
    }
}
