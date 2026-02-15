
using Domain.Entities.Production;
using Infrastructure.Persistance.EntityConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class WorkMasterBuilder : IEntityTypeConfiguration<WorkMaster>
{
    public const string TABLE_NAME = "WorkMaster";

    public void Configure(EntityTypeBuilder<WorkMaster> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.BaseQuantity)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                           ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.operatorCost)
            .IsRequired()
            .HasColumnType("decimal")
            .HasDefaultValue("0")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                           ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.machineCost)
            .IsRequired()
            .HasColumnType("decimal")
            .HasDefaultValue("0")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                           ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.externalCost)
            .IsRequired()
            .HasColumnType("decimal")
            .HasDefaultValue("0")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                           ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.materialCost)
            .IsRequired()
            .HasColumnType("decimal")
            .HasDefaultValue("0")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                           ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.totalWeight)
            .IsRequired()
            .HasColumnType("decimal")
            .HasDefaultValue("0")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                           ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.Mode)
            .IsRequired()
            .HasColumnType("integer")
            .HasDefaultValue("1");
        builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

        builder.ToTable(TABLE_NAME);
    }
}
