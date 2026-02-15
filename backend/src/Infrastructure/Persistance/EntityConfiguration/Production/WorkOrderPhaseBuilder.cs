using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class WorkOrderPhaseBuilder : IEntityTypeConfiguration<WorkOrderPhase>
{
    public const string TABLE_NAME = "WorkOrderPhase";
    public void Configure(EntityTypeBuilder<WorkOrderPhase> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.Code)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(10);
        builder
            .Property(b => b.Description)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(250);
        builder
            .Property(b => b.IsExternalWork)
            .HasColumnType("boolean");

        builder
            .Property(b => b.ExternalWorkCost)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
                          
        builder
            .Property(b => b.ProfitPercentage)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
                          
        builder
            .Property(b => b.TransportCost)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);

        builder
            .Property(b => b.QuantityOk)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);

        builder
            .Property(b => b.QuantityKo)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");

        builder.ToTable(TABLE_NAME);
    }
}
