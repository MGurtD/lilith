using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;

public class WorkOrderPhaseRejectionBuilder : IEntityTypeConfiguration<WorkOrderPhaseRejection>
{
    public const string TABLE_NAME = "WorkOrderPhaseRejections";

    public void Configure(EntityTypeBuilder<WorkOrderPhaseRejection> builder)
    {
        builder.ConfigureBase();

        builder
            .Property(b => b.Quantity)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);

        builder
            .HasOne(b => b.WorkOrderPhase)
            .WithMany(p => p.Rejections)
            .HasForeignKey(b => b.WorkOrderPhaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(b => b.RejectionReason)
            .WithMany()
            .HasForeignKey(b => b.RejectionReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(b => b.WorkcenterShiftDetail)
            .WithMany()
            .HasForeignKey(b => b.WorkcenterShiftDetailId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");

        builder
            .HasIndex(b => b.WorkOrderPhaseId)
            .HasDatabaseName($"IX_{TABLE_NAME}_WorkOrderPhaseId");

        builder
            .HasIndex(b => b.RejectionReasonId)
            .HasDatabaseName($"IX_{TABLE_NAME}_RejectionReasonId");

        builder.ToTable(TABLE_NAME);
    }
}
