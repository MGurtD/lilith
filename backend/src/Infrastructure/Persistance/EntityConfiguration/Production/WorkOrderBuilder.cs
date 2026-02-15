using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class WorkOrderBuilder : IEntityTypeConfiguration<WorkOrder>
{
    public const string TABLE_NAME = "WorkOrder";

    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ConfigureBase();
        builder
                .Property(b => b.Code)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(50);
        builder
            .Property(b => b.PlannedQuantity)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);

        builder
            .Property(b => b.TotalQuantity)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);

        builder
            .Property(b => b.StartTime)
            .HasColumnType("timestamp without time zone");
        builder
            .Property(b => b.EndTime)
            .HasColumnType("timestamp without time zone");
        builder
            .Property(b => b.Order)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnType("integer");
        builder
            .Property(b => b.Comment)
            .IsRequired()
            .HasDefaultValue("")
            .HasColumnType("varchar")
            .HasMaxLength(4000);

        builder
            .Property(b => b.OperatorCost)
            .IsRequired()
            .HasDefaultValue(0.0)
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.MachineCost)
            .IsRequired()
            .HasDefaultValue(0.0)
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);

        builder
            .Property(b => b.MaterialCost)
            .IsRequired()
            .HasDefaultValue(0.0)
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);

        builder
            .Property(b => b.OperatorTime)
            .IsRequired()
            .HasDefaultValue(0.0)
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.MachineTime)
            .IsRequired()
            .HasDefaultValue(0.0)
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");
        builder
            .HasIndex(builder => builder.Code, $"UK_{TABLE_NAME}")
            .IsUnique();

        builder.ToTable(TABLE_NAME);
    }
}
