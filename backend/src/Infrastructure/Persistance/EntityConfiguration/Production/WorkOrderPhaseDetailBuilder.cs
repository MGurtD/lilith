using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class WorkOrderPhaseDetailBuilder : IEntityTypeConfiguration<WorkOrderPhaseDetail>
{
    public const string TABLE_NAME = "WorkOrderPhaseDetail";
    public void Configure(EntityTypeBuilder<WorkOrderPhaseDetail> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.EstimatedTime)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.EstimatedOperatorTime)
            .IsRequired()
            .HasDefaultValue(0.0m)
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.IsCycleTime)
            .HasColumnType("boolean");       

        builder
            .Property(b => b.Order)
            .HasColumnType("integer");
        builder
            .Property(b => b.Comment)
            .IsRequired()
            .HasColumnType("text");

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");

        builder.ToTable(TABLE_NAME);
    }
}
