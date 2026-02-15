using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class WorkOrderPhaseBillOfMaterialsBuilder : IEntityTypeConfiguration<WorkOrderPhaseBillOfMaterials>
{
    public const string TABLE_NAME = "WorkOrderPhaseBillOfMaterials";
    public void Configure(EntityTypeBuilder<WorkOrderPhaseBillOfMaterials> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.Quantity)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
                .Property(b => b.Width)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.Length)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.Height)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.Diameter)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.Thickness)
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
