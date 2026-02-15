using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.EntityConfiguration.Production;

public class WorkcenterShiftDetailBuilder : IEntityTypeConfiguration<WorkcenterShiftDetail>
{
    public const string TableName = "WorkcenterShiftDetails";

    public void Configure(EntityTypeBuilder<WorkcenterShiftDetail> builder)
    {
        builder.ConfigureBaseWithoutTimestamps();

        builder
            .Property(b => b.Current)
            .IsRequired();
        builder
            .Property(b => b.StartTime)
            .IsRequired()
            .HasColumnType(ApplicationDbContextConstants.TIMESTAMP_WITHOUT_TIMEZONE);
        builder
            .Property(b => b.EndTime)
            .HasColumnType(ApplicationDbContextConstants.TIMESTAMP_WITHOUT_TIMEZONE);
        builder
            .Property(b => b.OperatorCost)
            .HasDefaultValue(0)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.WorkcenterCost)
            .HasDefaultValue(0)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.ConcurrentOperatorWorkcenters)
            .HasDefaultValue(0)
            .IsRequired()
            .HasColumnType("int");
        builder
            .Property(b => b.ConcurrentWorkorderPhases)
            .HasDefaultValue(0)
            .IsRequired()
            .HasColumnType("int");
        builder
            .Property(b => b.QuantityOk)
            .HasDefaultValue(0)
            .IsRequired()
            .HasColumnType("int");
        builder
            .Property(b => b.QuantityKo)
            .HasDefaultValue(0)
            .IsRequired()
            .HasColumnType("int");

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TableName}");
        builder
          .HasIndex(e => new { e.WorkcenterShiftId, e.MachineStatusId, e.OperatorId, e.WorkOrderPhaseId, e.StartTime })
          .HasDatabaseName($"IX_{TableName}_Unique") // Nombre opcional del índice
          .IsUnique(); // Opcional: índice único

        builder.ToTable(TableName, "data");
    }
}