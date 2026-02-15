using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Auth;

namespace Infrastructure.Persistance.EntityConfiguration.Auth
{
    public class UserBuilder : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Username)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Password)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.FirstName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.LastName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);
            builder
                .Property(b => b.PreferredLanguage)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10)
                .HasDefaultValue("ca");

            builder
                .HasKey(b => b.Id)
                .HasName("PK_Users");

            builder.HasIndex(builder => builder.Username, "UK_Users_Username");

            builder.ToTable("Users");
        }

    }
}
