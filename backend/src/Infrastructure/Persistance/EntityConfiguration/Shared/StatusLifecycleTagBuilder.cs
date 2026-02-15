using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Shared;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class StatusLifecycleTagBuilder : IEntityTypeConfiguration<StatusLifecycleTag>
    {
        public const string TABLE_NAME = "StatusLifecycleTags";

        public void Configure(EntityTypeBuilder<StatusLifecycleTag> builder)
        {
            builder.ConfigureBase();

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .ToTable(TABLE_NAME)
                .HasIndex(b => new { b.StatusId, b.LifecycleTagId })
                .IsUnique()
                .HasDatabaseName($"IX_{TABLE_NAME}_StatusId_LifecycleTagId");

            builder
                .HasOne(b => b.Status)
                .WithMany(s => s.LifecycleTags)
                .HasForeignKey(b => b.StatusId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(b => b.LifecycleTag)
                .WithMany(lt => lt.StatusTags)
                .HasForeignKey(b => b.LifecycleTagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
