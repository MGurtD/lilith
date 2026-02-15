using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class StatusBuilder : IEntityTypeConfiguration<Status>
    {
        public const string TABLE_NAME = "Statuses";

        public void Configure(EntityTypeBuilder<Status> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(new[] { "LifecycleId", "Name" }, $"UK_{TABLE_NAME}")
                .IsUnique();

            builder.ToTable(TABLE_NAME);
        }

    }
}
