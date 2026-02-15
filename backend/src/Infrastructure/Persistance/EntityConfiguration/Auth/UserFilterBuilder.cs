using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Auth;

namespace Infrastructure.Persistance.EntityConfiguration.Auth
{
    public class UserFilterBuilder : IEntityTypeConfiguration<UserFilter>
    {
        public void Configure(EntityTypeBuilder<UserFilter> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Page)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Key)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);
            builder
                .Property(b => b.Filter)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(4000);
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_UserFilter");

            builder.ToTable("UserFilters");
        }

    }
}
