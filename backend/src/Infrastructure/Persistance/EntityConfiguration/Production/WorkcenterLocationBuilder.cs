using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class WorkcenterLocationBuilder : IEntityTypeConfiguration<WorkcenterLocation>
    {
        public void Configure(EntityTypeBuilder<WorkcenterLocation> builder)
        {
            builder.ConfigureBase();
            builder
                .HasOne(b => b.Workcenter)
                .WithMany(w => w.WorkcenterLocations)
                .HasForeignKey(b => b.WorkcenterId)
                .OnDelete(DeleteBehavior.Cascade);
            builder
                .HasOne(b => b.Location)
                .WithMany()
                .HasForeignKey(b => b.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_WorkcenterLocation");
            builder.HasIndex(
                b => new { b.WorkcenterId, b.LocationId },
                "UK_WorkcenterLocation");

            builder.ToTable("WorkcenterLocations");
        }
    }
}
