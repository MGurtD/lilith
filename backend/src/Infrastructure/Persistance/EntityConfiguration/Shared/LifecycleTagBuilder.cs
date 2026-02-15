using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Shared;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class LifecycleTagBuilder : IEntityTypeConfiguration<LifecycleTag>
    {
        public const string TABLE_NAME = "LifecycleTags";

        public void Configure(EntityTypeBuilder<LifecycleTag> builder)
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
                .Property(b => b.Color)
                .HasColumnType("text");

            builder
                .Property(b => b.Icon)
                .HasColumnType("text");

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .ToTable(TABLE_NAME)
                .HasIndex(b => new { b.LifecycleId, b.Name })
                .IsUnique()
                .HasDatabaseName($"IX_{TABLE_NAME}_LifecycleId_Name");

            builder
                .HasOne(b => b.Lifecycle)
                .WithMany(l => l.Tags)
                .HasForeignKey(b => b.LifecycleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
